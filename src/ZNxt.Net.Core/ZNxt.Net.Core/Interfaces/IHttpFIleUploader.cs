using Newtonsoft.Json.Linq;
using System.Collections.Generic;

namespace ZNxt.Net.Core.Interfaces
{
    public interface IHttpFileUploader
    {
        List<string> GetFiles();

        string Save(string fileName, string destination = null, string fileBase64Data = null);

        JObject SaveToDB(IDBService dbProxy, string fileName, string baseFolder, string collection, string updateFilter = null, string fileBase64Data = null);

        byte[] GetFileData(string fileName);
    }
}