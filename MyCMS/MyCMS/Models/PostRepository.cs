using Microsoft.Extensions.Caching.Memory;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Table;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MyCMS.Models
{

    /// <summary>
    /// ブログの投稿のリポジトリ
    /// </summary>
    /// <remarks>
    /// * PartitionKey は POST とする。
    /// * RowKey は 現在時刻 の .ToString("yyyyMMddHHmmss") とする。重複する場合、もう一度時刻を取得する
    /// * RowKeyは1ユーザーが使っている分には重複してもさほど問題ないだろう。
    /// * import したデータについては、WordpressのPostIdをそのままRowKeyとして使う。
    /// </remarks>
    public class PostRepository : BaseRepository<Post>, IPostRepository
    {
        private const string CACHE_KEY_PUBLISHED_POSTS = "PUBLISHED_POSTS";
        private const string CACHE_KEY_ALL_POSTS = "ALL_POSTS";

        public IDateTimeProvider DateTimeProvider { get; }
        public IMemoryCache MemoryCache { get; }

        public PostRepository(
            IDateTimeProvider dateTimeProvider,
            IMemoryCache memoryCache
            )
        {
            this.DateTimeProvider = dateTimeProvider;
            this.MemoryCache = memoryCache;
        }

        public async Task<Post> GetAsync(string rowKey)
        {
            TableOperation retrieveOperation = TableOperation.Retrieve<Post>("POST", rowKey);
            var result = await this.Table.ExecuteAsync(retrieveOperation);
            return result.Result as Post;
        }

        public async Task<Post> GetByPostIdAsync(int postId)
        {
            string filter = TableQuery.CombineFilters(
                TableQuery.GenerateFilterCondition("PartitionKey", QueryComparisons.Equal, "POST"),
                TableOperators.And,
                TableQuery.GenerateFilterConditionForInt("WpPostId", QueryComparisons.Equal, postId)
            );
            TableQuery<Post> query = new TableQuery<Post>().Where(filter);
            var posts = await this.Table.ExecuteQuerySegmentedAsync(query, null);
            return posts.FirstOrDefault();
        }

        public async Task<IEnumerable<Post>> SearchPublishedPostsAsync()
        {
            IEnumerable<Post> posts = new Post[] { };

            if (!this.MemoryCache.TryGetValue(CACHE_KEY_PUBLISHED_POSTS, out posts))
            {
                string filter = TableQuery.CombineFilters(
                    TableQuery.GenerateFilterCondition("PartitionKey", QueryComparisons.Equal, "POST"),
                    TableOperators.And,
                    TableQuery.GenerateFilterCondition("Status", QueryComparisons.Equal, "publish")
                );
                TableQuery<Post> query = new TableQuery<Post>().Where(filter);

                posts = await this.Table.ExecuteQuerySegmentedAsync(query, null);

                var cacheEntryOptions = new MemoryCacheEntryOptions() {
                    AbsoluteExpiration = DateTimeOffset.Now.AddYears(1)
                };

                this.MemoryCache.Set(CACHE_KEY_PUBLISHED_POSTS, posts, cacheEntryOptions);
            }

            return posts;
        }

        public async Task<IEnumerable<Post>> SearchAsync()
        {
            IEnumerable<Post> posts = new Post[] { };

            if (!this.MemoryCache.TryGetValue(CACHE_KEY_ALL_POSTS, out posts))
            {
                string filter = TableQuery.GenerateFilterCondition("PartitionKey", QueryComparisons.Equal, "POST");
                TableQuery<Post> query = new TableQuery<Post>().Where(filter);

                posts = await this.Table.ExecuteQuerySegmentedAsync(query, null);

                var cacheEntryOptions = new MemoryCacheEntryOptions() {
                    AbsoluteExpiration = DateTimeOffset.Now.AddYears(1)
                };

                this.MemoryCache.Set(CACHE_KEY_ALL_POSTS, posts, cacheEntryOptions);
            }

            return posts;
        }

        /// <summary>
        /// postのRowKey,CreateDate,ModifyDateを設定して追加する
        /// </summary>
        /// <param name="post"></param>
        /// <returns></returns>
        public async Task AddAsync(Post post)
        {
            post.RowKey = (post.PublishDate?.ToString("yyyyMMddHHmmssfff") ?? "") + "_" + Guid.NewGuid().ToString();
            post.CreateDate = this.DateTimeProvider.Now;
            post.ModifyDate = this.DateTimeProvider.Now;

            TableOperation operation = TableOperation.Insert(post);

            await this.Table.ExecuteAsync(operation);

            this._ClearCache();
        }

        /// <summary>
        /// AddAsyncのCreateDateを設定しないバージョン。インポートで使用する
        /// </summary>
        /// <param name="post"></param>
        /// <returns></returns>
        public async Task AddWithoutSettingCreateDateAsync(Post post)
        {
            post.RowKey = (post.PublishDate?.ToString("yyyyMMddHHmmssfff") ?? "") + "_" + Guid.NewGuid().ToString();
            post.ModifyDate = this.DateTimeProvider.Now;
            TableOperation operation = TableOperation.Insert(post);

            await this.Table.ExecuteAsync(operation);

            this._ClearCache();
        }

        public async Task UpdateAsync(Post post)
        {
            await this.DeleteAsync(post);

            post.RowKey = (post.PublishDate?.ToString("yyyyMMddHHmmssfff") ?? "") + "_" + Guid.NewGuid().ToString();
            post.ModifyDate = this.DateTimeProvider.Now;

            TableOperation operation = TableOperation.Replace(post);

            await this.Table.ExecuteAsync(operation);

            this._ClearCache();
        }

        public async Task DeleteAsync(Post entity)
        {
            TableOperation deleteOperation = TableOperation.Delete(entity);
            await this.Table.ExecuteAsync(deleteOperation);

            this._ClearCache();
        }

        private void _ClearCache()
        {
            this.MemoryCache.Remove(CACHE_KEY_PUBLISHED_POSTS);
            this.MemoryCache.Remove(CACHE_KEY_ALL_POSTS);
        }

    }
}
