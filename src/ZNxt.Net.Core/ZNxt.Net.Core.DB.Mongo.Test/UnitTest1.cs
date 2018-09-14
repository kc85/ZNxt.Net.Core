using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json.Linq;
using ZNxt.Net.Core.Interfaces;

namespace ZNxt.Net.Core.DB.Mongo.Test
{
    [TestClass]
    public class UnitTest1
    {
        [TestMethod]
        public void TestMethod1()
        {
            IDBService dbService = new MongoDBService("DotNetCoreTest");
            JObject data = new JObject();
            data["name"] = "Khanin";


            dbService.WriteData("Test", data);

        }
    }
}
