using Moq;
using OmniCore.Model.Interfaces;
using OmniCore.Repository;
using OmniCore.Repository.Entities;
using System;
using System.Collections.Generic;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Text;
using Unity;

namespace OmniCore.Eros.Tests
{
    public static class ErosTestEnvironment
    {
        public static Guid PodId1 = new Guid("B44C2F49-9100-49B3-9BD0-C07AB8821576");
        public static Guid PodId2 = new Guid("B44C2F49-9100-49B3-9BD0-C07AB8821577");
        public static Guid PodId3 = new Guid("B44C2F49-9100-49B3-9BD0-C07AB8821578");
        public static Guid PodId4 = new Guid("B44C2F49-9100-49B3-9BD0-C07AB8821579");
        public static void SetupRepositories(IUnityContainer container)
        {
            var pods = new List<Pod>()
            {
                new Pod() { Id = 1, PodUniqueId = PodId1, Archived = false }
            };

            var mock = new Mock<PodRepository>();
            mock.Setup(x => x.GetActivePods())
                .ReturnsAsync(pods);
            mock.Setup(x => x.CreateOrUpdate(It.IsAny<Pod>()));

            container.RegisterInstance<PodRepository>(mock.Object) ;
        }

        public static void SetupRadioProvider(IUnityContainer container)
        {
            var mockPeripheralResult = new Mock<IRadioPeripheralScanResult>();

            var mockAdapter = new Mock<IRadioAdapter>();
            mockAdapter.Setup(
                a => a.ScanPeripherals(It.IsAny<Guid>()))
                    .Returns(
                        Observable.Create<IRadioPeripheralScanResult>( (IObserver<IRadioPeripheralScanResult> observer) =>
                        {
                            observer.OnNext(mockPeripheralResult.Object);
                            return Disposable.Empty;
                        }));

            var mockRadio = new Mock<Radio>();

            var mockProvider = new Mock<IRadioProvider>();
            mockProvider.Setup(p => p.ListRadios())
                .Returns(
                        Observable.Create<Radio>((IObserver<Radio> observer) =>
                        {
                            observer.OnNext(mockRadio.Object);
                            return Disposable.Empty;
                        }));

            container.RegisterInstance<IRadioProvider>(mockProvider.Object);
        }
    }
}
