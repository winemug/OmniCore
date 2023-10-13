using OmniCore.Client.Mobile.Implementations;

namespace OmniCore.Client.Tests
{
    [TestClass]
    public class UnitTest1
    {
        [TestMethod]
        public void TestMethod1()
        {
            var bolus = new PodUnits(1.80m, 200);
            var rate = new PodUnitRate(new PodUnits(33.10m, 200), 3);
            Assert.AreEqual(bolus.MilliPulses, 27000m);
        }
    }
}