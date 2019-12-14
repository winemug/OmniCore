using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace OmniCore.Model.Interfaces.Platform
{
    public interface IRadioPeripheralCharacteristic : IDisposable
    {
        Guid Uuid { get; }
        Task<byte[]> Read(CancellationToken cancellationToken);
        Task Write(byte[] data, CancellationToken cancellationToken);
        IObservable<IRadioPeripheralCharacteristic> WhenNotificationReceived();
    }
}
