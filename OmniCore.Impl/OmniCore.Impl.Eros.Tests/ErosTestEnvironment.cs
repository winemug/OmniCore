using Moq;
using OmniCore.Model.Interfaces;
using System;
using System.Collections.Generic;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Text;
using Unity;

namespace OmniCore.Impl.Eros.Tests
{
    public static class ErosTestEnvironment
    {
        public static Guid PodId1 = new Guid("B44C2F49-9100-49B3-9BD0-C07AB8821576");
        public static Guid PodId2 = new Guid("B44C2F49-9100-49B3-9BD0-C07AB8821577");
        public static Guid PodId3 = new Guid("B44C2F49-9100-49B3-9BD0-C07AB8821578");
        public static Guid PodId4 = new Guid("B44C2F49-9100-49B3-9BD0-C07AB8821579");
        public static void SetupRepositories(IUnityContainer container)
        {
            var pods = new List<ErosPod>()
            {
                new ErosPod() { Id = PodId1, Archived = false }
            };

            var mock = new Mock<IPodRepository<ErosPod>>();
            mock.Setup(x => x.GetActivePods())
                .ReturnsAsync(pods);
            mock.Setup(x => x.SavePod(It.IsAny<ErosPod>()));

            container.RegisterInstance<IPodRepository<ErosPod>>(mock.Object) ;
        }

        public static void SetupRadioProvider(IUnityContainer container)
        {
            var mockPeripheral = new Mock<IRadioPeripheral>();

            var mockAdapter = new Mock<IRadioAdapter>();
            mockAdapter.Setup(
                a => a.ScanPeripherals(It.IsAny<Guid>()))
                    .Returns(
                        Observable.Create<IRadioPeripheral>( (IObserver<IRadioPeripheral> observer) =>
                        {
                            observer.OnNext(mockPeripheral.Object);
                            return Disposable.Empty;
                        }));

            var mockRadio = new Mock<IRadio>();

            var mockProvider = new Mock<IRadioProvider>();
            mockProvider.Setup(p => p.ListRadios())
                .Returns(
                        Observable.Create<IRadio>((IObserver<IRadio> observer) =>
                        {
                            observer.OnNext(mockRadio.Object);
                            return Disposable.Empty;
                        }));

            container.RegisterInstance<IRadioProvider>(mockProvider.Object);
        }
    }
}
