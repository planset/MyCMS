using System.IO;
using System.Threading.Tasks;

namespace MyCMS.Models
{
    public class FileInfo
    {
        public string LinkUrl { get; set; }
    }

    public interface IFileStorage
    {
        Task<FileInfo> AddAsync(string fileName, Stream stream);
    }
}
