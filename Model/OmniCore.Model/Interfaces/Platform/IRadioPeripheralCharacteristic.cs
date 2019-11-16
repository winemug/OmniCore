using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace OmniCore.Model.Interfaces.Platform
{
    public interface IRadioPeripheralCharacteristic : IDisposable
    {
        Task<byte[]> Read(TimeSpan timeout, CancellationToken cancellationToken);
        Task Write(byte[] data, TimeSpan timeout, CancellationToken cancellationToken);
        IObservable<IRadioPeripheralCharacteristic> WhenNotificationReceived();
    }
}
