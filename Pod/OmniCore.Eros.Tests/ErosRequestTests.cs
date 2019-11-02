using Microsoft.VisualStudio.TestTools.UnitTesting;
using OmniCore.Model;
using OmniCore.Repository.Enums;
using OmniCore.Model.Interfaces;
using OmniCore.Eros;
using System;
using System.Diagnostics;
using System.Threading.Tasks;
using Unity;

namespace OmniCore.Eros.Tests
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
            var provider = Container.Resolve<IPodProvider>();
            var pod = await provider.GetActivePod();
        }
    }
}
