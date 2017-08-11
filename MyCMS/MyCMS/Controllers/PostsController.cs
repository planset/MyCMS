using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using System.Xml;
using System.Xml.Linq;
using System.Net.Http;
using System.Text.RegularExpressions;
using MyCMS.Models;
using MyCMS.ViewModels;

namespace MyCMS.Controllers
{
    [Route("api/[controller]")]
    [Authorize]
    public class PostsController : Controller
    {
        private IPostRepository PostRepository { get; }
        private IFileStorage FileStorage { get; }

        public PostsController(
            IPostRepository postRepository,
            IFileStorage fileStorage)
        {
            this.PostRepository = postRepository;
            this.FileStorage = fileStorage;
        }

        [AllowAnonymous]
        [HttpGet]
        public async Task<IEnumerable<Post>> Get()
        {
            var posts = await this.PostRepository.SearchPublishedPostsAsync();

            return posts.OrderByDescending(x => x.PublishDate).Take(20);
        }

        [HttpGet]
        [Route("allposts")]
        public async Task<IEnumerable<Post>> GetAllPosts()
        {
            var posts = await this.PostRepository.SearchAsync();

            return posts.OrderByDescending(x => x.CreateDate).Take(20);
        }

        [AllowAnonymous]
        [HttpGet("{rowKey}")]
        public async Task<Post> Get(string rowKey)
        {
            return await this.PostRepository.GetAsync(rowKey);
        }

        [HttpPost]
        public async Task Post([FromBody]Post post)
        {
            if (this.ModelState.IsValid)
            {
                post.PublishDate = DateTime.UtcNow;
                post.Status = "publish";
                await this.PostRepository.AddAsync(post);
            }
        }

        [HttpPut("{rowKey}")]
        public async Task Put(string rowKey, [FromBody]Post newPost)
        {
            var post = await this.PostRepository.GetAsync(rowKey);
            post.Title = newPost.Title;
            post.Content = newPost.Content;
            await this.PostRepository.UpdateAsync(post);
        }

        [HttpDelete("{rowKey}")]
        public async Task Delete(string rowKey)
        {
            var post = await this.PostRepository.GetAsync(rowKey);
            await this.PostRepository.DeleteAsync(post);
        }

        [Route("/api/posts/import")]
        [HttpPost]
        public async Task<PostImportApiResult> Import()
        {
            if (!this.ModelState.IsValid)
            {
                return ApiHelpers.CreateApiResult<PostImportApiResult>(ApiCode.BadRequest);
            }

            IFormFile importFile = this.Request.Form.Files.FirstOrDefault();
            if (importFile == null)
            {
                return ApiHelpers.CreateApiResult<PostImportApiResult>(ApiCode.BadRequest);
            }

            var xmlString = "";
            using (var sr = new System.IO.StreamReader(importFile.OpenReadStream()))
            {
                xmlString = await sr.ReadToEndAsync();
            }

            await this.ImportWordpressPostsFromXmlString(xmlString);

            return ApiHelpers.CreateApiResult<PostImportApiResult>(ApiCode.Success);
        }

        private async Task ImportWordpressPostsFromXmlString(string xmlString)
        {
            var attachmentDict = new Dictionary<string, string>();
            var regexAttachment = new Regex(@"(http[s]?:)?//dkpyn.com/blog/wp-content/.*?(.jpg|.png)", RegexOptions.IgnoreCase);

            var root = XDocument.Parse(xmlString, LoadOptions.None)?.Root;
            var channel = root.Element("channel");

            //
            // post_typeでリストわけ
            //
            var attachments = new List<XElement>();
            var posts = new List<XElement>();

            foreach (var item in channel.Elements("item"))
            {
                var elements = item.Elements().ToArray();
                var postType = elements.FirstOrDefault(x => x.Name.LocalName == "post_type");

                if (postType != null && postType.Value == "attachment")
                {
                    attachments.Add(item);
                }

                if (postType != null && postType.Value == "post")
                {
                    posts.Add(item);
                }
            }

            //
            // 添付ファイルの取り込み
            //
            foreach (var item in attachments)
            {
                var elements = item.Elements().ToArray();
                var attachment_url = elements.FirstOrDefault(x => x.Name.LocalName == "attachment_url").Value;
                var postId = int.Parse(elements.FirstOrDefault(x => x.Name.LocalName == "post_id").Value);
                var ext = System.IO.Path.GetExtension(attachment_url);

                using (var client = new HttpClient())
                using (var response = await client.GetAsync(attachment_url))
                {
                    if (response.StatusCode == System.Net.HttpStatusCode.OK)
                    {
                        using (var stream = await response.Content.ReadAsStreamAsync())
                        {
                            var fi = await this.FileStorage.AddAsync(postId.ToString() + ext, stream);
                            attachmentDict[attachment_url] = fi.LinkUrl;
                        }
                    }
                }
            }

            //
            // 記事の取り込み
            //
            foreach (var item in posts)
            {
                var elements = item.Elements().ToArray();

                try
                {
                    var title = elements.FirstOrDefault(x => x.Name.LocalName == "title").Value;
                    var link = elements.FirstOrDefault(x => x.Name.LocalName == "link").Value;
                    var pubDate = elements.FirstOrDefault(x => x.Name.LocalName == "pubDate").Value;
                    var creator = elements.FirstOrDefault(x => x.Name.LocalName == "creator").Value;
                    var content = elements.FirstOrDefault(x => x.Name.NamespaceName == "http://purl.org/rss/1.0/modules/content/").Value;
                    var postId = int.Parse(elements.FirstOrDefault(x => x.Name.LocalName == "post_id").Value);
                    var postDateGmt = elements.FirstOrDefault(x => x.Name.LocalName == "post_date_gmt").Value;
                    var status = elements.FirstOrDefault(x => x.Name.LocalName == "status").Value;
                    var tags = new List<string>();
                    var cats = new List<string>();
                    foreach (var cat in elements.Where(x => x.Name.LocalName == "category"))
                    {
                        if (cat.Attribute("domain").Value == "post_tag")
                        {
                            tags.Add(cat.Value);
                        }
                        else if (cat.Attribute("domain").Value == "category")
                        {
                            cats.Add(cat.Value);
                        }
                    }

                    // 添付ファイルのURLをAzureStorageBlobのURLに置換
                    foreach (var m in regexAttachment.Matches(content))
                    {
                        if (attachmentDict.Any(x => x.Key == m.ToString()))
                        {
                            content = content.Replace(m.ToString(), attachmentDict[m.ToString()]);
                        }
                    }

                    // AzureStorageTableに追加(同じPostIdのデータは削除する）
                    var post = await this.PostRepository.GetByPostIdAsync(postId);
                    if (post != null)
                    {
                        await this.PostRepository.DeleteAsync(post);
                    }

                    post = new Models.Post()
                    {
                        Title = title,
                        Content = content
                    };

#pragma warning disable IDE0018 // Inline variable declaration
                    DateTime dt;
#pragma warning restore IDE0018 // Inline variable declaration
                    if (DateTime.TryParse(pubDate, out dt))
                    {
                        post.PublishDate = dt;
                    }
                    post.CreateDate = DateTime.Parse(postDateGmt);
                    post.Status = status;
                    post.Categories = String.Join(",", cats);
                    post.Tags = String.Join(",", tags);
                    post.WpLink = link;
                    post.WpPostId = postId;

                    await this.PostRepository.AddWithoutSettingCreateDateAsync(post);

                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                    continue;
                }
            }
        }
    }
}
