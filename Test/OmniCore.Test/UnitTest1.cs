using Moq;
using Nito.AsyncEx;
using OmniCore.Mobile.Services;
using OmniCore.Services;
using OmniCore.Services.Interfaces;
using OmniCore.Services.Interfaces.Definitions;
using OmniCore.Services.Interfaces.Entities;
using Unity;
using Unity.Lifetime;
using Initializer = OmniCore.Mobile.Initializer;

namespace OmniCore.Test;

public class Tests
{
    private IUnityContainer _container;

    [SetUp]
    public void Setup()
    {
        _container = new UnityContainer();
    }

    [Test]
    public void ContainerResolves()
    {
        Initializer.RegisterTypes(_container);
        var ns = _container.Resolve<NavigationService>();
        Assert.NotNull(ns);
    }

    [Test]
    public void PodTest1()
    {
        var p = new RequestInsulinSchedulePart(
            new BasalRateEntry
            {
                HalfHourCount = 24,
                PulsesPerHour = 600
            });

        Assert.That(p.Data.ToString(), Is.EqualTo("0104F5183840012C712C"));
    }

    [Test]
    public async Task QueueT1()
    {
        var q = new AsyncProducerConsumerQueue<int>();
        await q.EnqueueAsync(3);
        await q.EnqueueAsync(5);
        await q.EnqueueAsync(9);
        while (await q.OutputAvailableAsync())
        {
            var z = q.DequeueAsync();
        }
    }

    [Test]
    public async Task TestExchange()
    {
        var podServiceMock = new Mock<IPodService>();
        var radioServiceMock = new Mock<IRadioService>();
        var dataServiceMock = new Mock<IDataService>();

        _container.RegisterInstance(dataServiceMock.Object, new ContainerControlledLifetimeManager());
        _container.RegisterInstance(podServiceMock.Object, new ContainerControlledLifetimeManager());
        _container.RegisterInstance(radioServiceMock.Object, new ContainerControlledLifetimeManager());

        var podService = podServiceMock.Object;
        var dataService = dataServiceMock.Object;
        var pod = new Pod(dataService)
        {
            Id = Guid.NewGuid(),
            Medication = MedicationType.Insulin,
            RadioAddress = 0x34000000,
            UnitsPerMilliliter = 100,
            ValidFrom = DateTimeOffset.Now,
            Info = new PodRuntimeInformation()
        };


        using (var conn = await podService.GetConnectionAsync(pod))
        {
        }
    }
}