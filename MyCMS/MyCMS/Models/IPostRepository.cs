using Microsoft.Extensions.Caching.Memory;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Table;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MyCMS.Models
{

    public interface IPostRepository
    {
        Task<Post> GetAsync(string rowKey);
        Task<Post> GetByPostIdAsync(int postId);
        Task AddAsync(Post post);
        Task AddWithoutSettingCreateDateAsync(Post post);
        Task UpdateAsync(Post post);
        Task DeleteAsync(Post post);

        Task<IEnumerable<Post>> SearchPublishedPostsAsync();
        Task<IEnumerable<Post>> SearchAsync();
    }

}
