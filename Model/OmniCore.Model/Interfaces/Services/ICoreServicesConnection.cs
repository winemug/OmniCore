using System;

namespace OmniCore.Model.Interfaces.Platform
{
    public interface ICoreServicesConnection
    {
        IObservable<ICoreServicesConnection> WhenDisconnected { get; }
        IObservable<ICoreServices> WhenConnected { get; }
    }
}