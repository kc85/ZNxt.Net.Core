using System.Threading.Tasks;

namespace ZNxt.Net.Core.Interfaces
{
    public interface IStaticContentHandler
    {
        Task<string> GetStringContentAsync(string path);

        Task<byte[]> GetContentAsync(string path);
    }
}