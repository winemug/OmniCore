using System;
using System.Buffers;
using System.Threading.Tasks;
using NUnit.Framework;
using OmniCore.Model.Entities;
using OmniCore.Model.Enumerations;
using OmniCore.Model.Interfaces;
using OmniCore.Model.Interfaces.Services;
using OmniCore.Model.Interfaces.Services.Internal;
using OmniCore.Model.Utilities;
using OmniCore.Radios.RileyLink;
using Unity;

namespace OmniCore.Eros.Test
{
    public class Requests
    {
        private IContainer Container;
        private IErosPod Pod;
        [SetUp]
        public void Setup()
        {
            Container = new Container(new UnityContainer())
                .WithOmnipodEros();

            var repo = new Moq.Mock<IRepositoryService>();
            var tp = new Moq.Mock<ITaskProgress>();
            var repoReadOnly = new Moq.Mock<IRepositoryContextReadOnly>();
            var repoReadWrite = new Moq.Mock<IRepositoryContextReadWrite>();

            Container
                .Existing(repo.Object)
                .Existing(tp.Object)
                .Existing(repoReadOnly.Object)
                .Existing(repoReadWrite.Object);

            var podEntity = new PodEntity();
            var pod = new Moq.Mock<IErosPod>();
            pod.Setup(p => p.Entity)
                .Returns(podEntity);
            
            Pod = pod.Object;
        }

        [Test]
        public async Task PairRequest1()
        {
            var podRequest = await Container.Get<IErosPodRequest>();
            podRequest
                .WithPairRequest(0x33131415);

            Console.WriteLine(new Bytes(podRequest.Message).ToString());
        }

        [Test]
        public async Task Response1()
        {
            var podResponse = await Container.Get<IErosPodResponse>();
            var responseData = new Bytes("001122334455");
            podResponse.ParseResponse(responseData.ToArray());
            Assert.IsFalse(podResponse.IsValid);
        }

        [Test]
        public async Task odiz()
        {
            var podRequest = await Container.Get<IErosPodRequest>();
            podRequest
                .WithPod(Pod)
                .WithPairRequest(0x33131415);
            
            var prc = PacketRadioConversation.ForRequest(podRequest);
            
            Console.WriteLine(new Bytes(prc.GetPacketToSend(false)).ToString());

            prc.ParseIncomingPacket(null, false);
            Console.WriteLine(new Bytes(prc.GetPacketToSend(false)).ToString());

            prc.ParseIncomingPacket(null, false);
            Console.WriteLine(new Bytes(prc.GetPacketToSend(false)).ToString());

            var i1 = new RadioPacket(0xffffffff, 01, PacketType.ACK, null)
                .GetPacketData(false);
            prc.ParseIncomingPacket(i1, false);
            Console.WriteLine(new Bytes(prc.GetPacketToSend(false)).ToString());
            
        }
    }
}