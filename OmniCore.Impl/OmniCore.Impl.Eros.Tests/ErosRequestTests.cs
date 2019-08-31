using Microsoft.VisualStudio.TestTools.UnitTesting;
using OmniCore.Impl.Eros.Requests;
using OmniCore.Model;
using OmniCore.Model.Enums;
using OmniCore.Model.Interfaces;
using System;
using System.Diagnostics;
using System.Threading.Tasks;
using Unity;

namespace OmniCore.Impl.Eros.Tests
{
    [TestClass]
    public class ErosRequestTests : TestBase
    {
        [TestMethod]
        public async Task SendStatusRequest()
        {

        }

        [TestMethod]
        public async Task PairRequest()
        {
            var provider = Container.Resolve<IPodProvider<ErosPod>>();
            var pod = await provider.GetActivePod();
        }
    }
}
