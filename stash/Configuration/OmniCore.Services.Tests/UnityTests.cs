using System.Threading.Tasks;
using Moq;
using NUnit.Framework;
using OmniCore.Model.Interfaces.Platform.Common;
using OmniCore.Repository.Sqlite;
using Unity;

namespace OmniCore.Services.Tests
{
    public class UnityTests
    {
        //private IUnityContainer Container;
        //[SetUp]
        //public void Setup()
        //{
        //    var appService = new Mock<ICoreApplicationFunctions>().Object;

        //    Container = new UnityContainer()
        //        .WithSqliteRepositories()
        //        .RegisterInstance<ICoreApplicationFunctions>(appService);
        //}

        //[Test]
        //public async Task Test1()
        //{
        //    var provider = Container.Resolve<ICoreServicesProvider>();
        //    var services = provider.LocalServices;
        //    Assert.Pass();
        //}
    }
}