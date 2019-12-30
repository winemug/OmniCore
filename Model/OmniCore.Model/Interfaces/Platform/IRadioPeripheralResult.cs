using System.ComponentModel;

namespace OmniCore.Model.Interfaces.Services
{
    public interface IRadioPeripheralResult : INotifyPropertyChanged
    {
        IRadioPeripheral Peripheral { get; }
        int? Rssi { get; set; }
    }
}
