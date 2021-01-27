using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Threading.Tasks;
using TesonetTask;

namespace TesonetTaskTest
{
    [TestClass]
    public class DataTest
    {
        [TestMethod]
        public void LogTest()
        {
            Logger.LogLocation = "log.txt";
            Logger.Log("Test message");
        }

        [TestMethod]
        public async Task FetchTest()
        {
            var dataManager = new DataManager("https://playground.tesonet.lt/v1/tokens", "https://playground.tesonet.lt/v1/servers", null);
            var token = await dataManager.GetAuthToken("tesonet", "partyanimal");
            var servers = await dataManager.GetServerList(token);
            Assert.IsNotNull(servers);
        }
    }
}
