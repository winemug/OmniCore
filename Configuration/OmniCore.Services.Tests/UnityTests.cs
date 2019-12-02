using System.Threading.Tasks;
using NUnit.Framework;
using OmniCore.Model.Interfaces.Services;
using OmniCore.Repository.Sqlite;
using Unity;

namespace OmniCore.Services.Tests
{
    public class UnityTests
    {
        private IUnityContainer Container;
        [SetUp]
        public void Setup()
        {
            Container = new UnityContainer()
                .WithDefaultServiceProviders()
                .WithSqliteRepository();
        }

        [Test]
        public async Task Test1()
        {
            var provider = Container.Resolve<ICoreServicesProvider>();
            var services = provider.LocalServices;
            Assert.Pass();
        }
    }
}