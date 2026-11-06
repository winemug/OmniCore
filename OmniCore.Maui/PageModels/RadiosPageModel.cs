using System.Collections.ObjectModel;
using CommunityToolkit.Maui.Core.Extensions;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using OmniCore.Maui.Models;
using OmniCore.Maui.Services;

namespace OmniCore.Maui.PageModels;

public partial class RadiosPageModel(BleService bleService) : PageModel
{
    public ObservableCollection<BleDevice> Radios = new();

    protected override async Task Appearing()
    {
        Radios.Clear();
        // Radios.Add(
        //     new BleDeviceModel(Guid.NewGuid(), "radio1")
        // );
    }
    
    [RelayCommand]
    private async Task StartScan()
    {
        bleService.StartScanning();
    }

    protected override Task NavigatedFrom()
    {
        return base.NavigatedFrom();
    }
}
