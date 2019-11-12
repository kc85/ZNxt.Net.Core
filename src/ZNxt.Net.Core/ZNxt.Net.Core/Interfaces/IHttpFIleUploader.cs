using System.Collections.Generic;
using System.IO;

namespace ZNxt.Net.Core.Interfaces
{
    public interface IHttpFileUploader
    {
        List<string> GetFiles();

        byte[] GetFileData(string fileName);

        Stream GetFileStream(string fileName);
    }
}