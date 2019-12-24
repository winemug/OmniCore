using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using OmniCore.Model.Enumerations;
using OmniCore.Model.Interfaces.Platform;

namespace OmniCore.Simulation.Radios
{
    public class RadioPeripheral : IRadioPeripheral
    {
        public void Dispose()
        {
        }

        public Guid Uuid { get; }

        public string Name
        {
            get;
            set;
        }
        public async Task<IRadioPeripheralLease> Lease(CancellationToken cancellationToken)
        {
            return new RadioPeripheralLease();
        }

        public TimeSpan? RssiUpdateTimeSpan { get; set; }
        public int? Rssi { get; set; }
        public DateTimeOffset? RssiDate { get; }
        
        public PeripheralConnectionState ConnectionState { get; }
        public DateTimeOffset? ConnectionStateDate { get; }
        public DateTimeOffset? DisconnectDate { get; }

        public event PropertyChangedEventHandler PropertyChanged;
    }
}
