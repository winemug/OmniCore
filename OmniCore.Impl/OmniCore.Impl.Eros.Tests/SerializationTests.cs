using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;
using OmniCore.Impl.Eros.Requests;
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
            var jsonStr = req.ToJson();
            Console.WriteLine(jsonStr);

            var parsedRequest = GetRequest(jsonStr);

            Console.WriteLine(parsedRequest);
        }

        private IPodRequest GetRequest(string json)
        {
            dynamic parsedRequest = JsonConvert.DeserializeObject(json);
            RequestType? rt = parsedRequest.PodRequestType;
            switch (rt)
            {
                case RequestType.Pair:
                    return JsonConvert.DeserializeObject<ErosRequestPair>(json);
                default:
                    return null;
            }
        }
    }
}
