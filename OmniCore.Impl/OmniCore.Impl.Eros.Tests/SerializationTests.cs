using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;
using OmniCore.Model;
using OmniCore.Model.Enums;
using OmniCore.Model.Interfaces;
using System;
using System.Diagnostics;
using System.Threading.Tasks;

namespace OmniCore.Impl.Eros.Tests
{
    [TestClass]
    public class SerializationTests
    {
        [TestMethod]
        public async Task Test1()
        {
            var podId = Guid.NewGuid();
            var pod = new ErosPod() { Id = podId } ;
            var req = await pod.CreatePairRequest(0x33002211);
            var jsonStr = JsonConvert.ToString(req);
            Console.WriteLine(jsonStr);

            var parsedRequest = GetRequest(jsonStr);

            Console.WriteLine(parsedRequest);
        }

        private IPodRequest GetRequest(string json)
        {
            return JsonConvert.DeserializeObject<ErosRequest>(json);
        }
    }
}
