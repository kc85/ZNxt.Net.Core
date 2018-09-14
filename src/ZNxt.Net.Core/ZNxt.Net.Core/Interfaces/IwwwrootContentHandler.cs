namespace ZNxt.Net.Core.Interfaces
{
    public interface IWwwrootContentHandler
    {
        string GetStringContent(string path);

        byte[] GetContent(string path);
    }
}