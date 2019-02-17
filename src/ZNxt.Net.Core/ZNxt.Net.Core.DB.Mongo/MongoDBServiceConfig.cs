using ZNxt.Net.Core.Config;
using ZNxt.Net.Core.Helpers;
using ZNxt.Net.Core.Interfaces;

namespace ZNxt.Net.Core.DB.Mongo
{
    public class MongoDBServiceConfig : IDBServiceConfig
    {
        public string DBName { get; private set; }

        public string ConnectingString { get; private set; }
        public MongoDBServiceConfig()
        {
            
            DBName = ApplicationConfig.DataBaseName;
            ConnectingString = ApplicationConfig.ConnectionString;
        }

        public void Set(string dbName, string connectingString)
        {
            DBName = dbName;
            ConnectingString = connectingString;
        }

        public void Save()
        {
            CommonUtility.SaveConfig("DataBaseName", this.DBName);
            CommonUtility.SaveConfig("ConnectionString", this.ConnectingString);
        }
    }
}
