using System;

namespace OmniCore.Model.Interfaces.Platform
{
    public interface ICoreServicesConnection
    {
        bool Connect();
        void Disconnect();
        IObservable<ICoreServicesConnection> WhenDisconnected();
        IObservable<ICoreServices> WhenConnected();
        ICoreServices CoreServices { get; }
    }
}