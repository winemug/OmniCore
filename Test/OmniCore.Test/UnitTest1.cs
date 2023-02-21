using Moq;
using Nito.AsyncEx;
using OmniCore.Mobile;
using OmniCore.Mobile.Services;
using OmniCore.Services;
using OmniCore.Services.Interfaces;
using Unity;
using Xamarin.Forms;

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
        OmniCore.Mobile.Initializer.RegisterTypes(_container);
        var ns = _container.Resolve<NavigationService>();
        Assert.NotNull(ns);
    }

    [Test]
    public void PodTest1()
    {
        var p = new RequestInsulinSchedulePart(
            new BasalRateEntry()
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
}