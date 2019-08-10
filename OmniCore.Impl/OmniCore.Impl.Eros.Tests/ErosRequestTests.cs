using Microsoft.VisualStudio.TestTools.UnitTesting;
using OmniCore.Impl.Eros.Requests;
using OmniCore.Model;
using OmniCore.Model.Enums;
using OmniCore.Model.Interfaces;

namespace OmniCore.Impl.Eros.Tests
{
    [TestClass]
    public class ErosRequestTests
    {
        [TestMethod]
        public async void SendStatusRequest()
        {
            var pp = new ErosPodProvider(new TestRadioAdapter());
            var pod = await pp.GetActivePod();

            var req = new ErosStatusRequest(StatusRequestType.Standard);
            var result = await pod.Request(req);
            var result2 = await pod.Cancel(req);
        }
    }
}
