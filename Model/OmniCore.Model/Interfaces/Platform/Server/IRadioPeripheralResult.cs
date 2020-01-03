using System.ComponentModel;

namespace OmniCore.Model.Interfaces.Platform
{
    public interface IRadioPeripheralResult : INotifyPropertyChanged, IServerResolvable
    {
        IRadioPeripheral Peripheral { get; }
        int? Rssi { get; set; }
    }
}
