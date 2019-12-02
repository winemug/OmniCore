using System.Threading.Tasks;
using Moq;
using NUnit.Framework;
using OmniCore.Model.Interfaces.Services;
using OmniCore.Model.Interfaces.Platform;
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
            var uiApp = new Mock<IUserInterface>().Object;
            var appService = new Mock<IApplicationService>().Object;

            Container = new UnityContainer()
                .WithDefaultServiceProviders()
                .WithSqliteRepository()
                .RegisterInstance<IUserInterface>(uiApp)
                .RegisterInstance<IApplicationService>(appService);
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