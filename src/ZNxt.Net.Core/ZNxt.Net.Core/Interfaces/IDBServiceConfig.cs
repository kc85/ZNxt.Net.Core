namespace ZNxt.Net.Core.Interfaces
{
    public interface IDBServiceConfig
    {
        string DBName { get; }
        string ConnectingString { get; }
        void Set(string dbName, string connectingString);
        void Save();
    }
}
