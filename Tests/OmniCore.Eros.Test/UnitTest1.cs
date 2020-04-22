using System;
using System.Buffers;
using System.Threading.Tasks;
using NUnit.Framework;
using OmniCore.Model.Enumerations;
using OmniCore.Model.Interfaces.Common;
using OmniCore.Model.Interfaces.Services;
using OmniCore.Model.Interfaces.Services.Facade;
using OmniCore.Model.Interfaces.Services.Internal;
using OmniCore.Model.Utilities;
using Unity;

namespace OmniCore.Eros.Test
{
    public class Tests
    {
        private IContainer Container;
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
        }

        [Test]
        public async Task Test1()
        {
            var podRequest = await Container.Get<IErosPodRequest>();
            podRequest
                .WithMessageAddress(0x1f0e89ee)
                .WithMessageSequence(0)
                .WithStatusRequest(StatusRequestType.Standard)
                .WithPairRequest(0x1f0e89ee);

            Console.WriteLine(new Bytes(podRequest.Message).ToString());
        }
    }
}