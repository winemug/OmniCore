using System.ComponentModel;

namespace OmniCore.Model.Interfaces.Platform
{
    public interface IRadioPeripheralResult : INotifyPropertyChanged
    {
        IRadioPeripheral Peripheral { get; }
        int? Rssi { get; set; }
    }
}
