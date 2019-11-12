using System.Collections.Generic;
using System.IO;
using ZNxt.Net.Core.Interfaces;

namespace ZNxt.Net.Core.Web.Proxies
{
    public partial class HttpContextProxy : IHttpFileUploader
    {
        public List<string> GetFiles()
        {
            List<string> files = new List<string>();
            
            for (int i = 0; i < _httpContextAccessor.HttpContext.Request.Form.Files.Count; i++)
            {
                files.Add(_httpContextAccessor.HttpContext.Request.Form.Files[i].FileName);
            }
            return files;
        }

        public byte[] GetFileData(string fileName)
        {
            for (int i = 0; i < _httpContextAccessor.HttpContext.Request.Form.Files.Count; i++)
            {
                if (_httpContextAccessor.HttpContext.Request.Form.Files[i].FileName == fileName)
                {
                    BinaryReader b = new BinaryReader(_httpContextAccessor.HttpContext.Request.Form.Files[i].OpenReadStream());
                    byte[] binData = b.ReadBytes((int)_httpContextAccessor.HttpContext.Request.Form.Files[i].Length);
                    return binData;
                }
            }
            return null;
        }
        public Stream GetFileStream(string fileName)
        {
            for (int i = 0; i < _httpContextAccessor.HttpContext.Request.Form.Files.Count; i++)
            {
                if (_httpContextAccessor.HttpContext.Request.Form.Files[i].FileName == fileName)
                {
                    return _httpContextAccessor.HttpContext.Request.Form.Files[i].OpenReadStream();
                }
            }
            return null;
        }

    }
}
