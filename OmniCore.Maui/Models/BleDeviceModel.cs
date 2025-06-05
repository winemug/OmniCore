using CommunityToolkit.Mvvm.ComponentModel;

namespace OmniCore.Maui.Models;

public partial class BleDeviceModel : ObservableObject
{
    [ObservableProperty]
    public string _name;
    
    [ObservableProperty]
    public Guid _id;

    public BleDeviceModel(Guid id, string name)
    {
        _id = id;
        _name = name;
    }
}