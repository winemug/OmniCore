using System.Collections.ObjectModel;
using CommunityToolkit.Maui.Core.Extensions;
using CommunityToolkit.Mvvm.ComponentModel;
using OmniCore.Maui.Models;
using OmniCore.Maui.Services;

namespace OmniCore.Maui.PageModels;

public partial class RadiosPageModel(BleService bleService) : PageModel
{
    [ObservableProperty] private ObservableCollection<BleDevice> _radios;

    protected override async Task Appearing()
    {
        Radios = bleService.BleDevices;
        bleService.StartScanning();
        // Radios.Clear();
        // Radios.Add(
        //     new BleDeviceModel(Guid.NewGuid(), "radio1")
        // );
    }

    protected override Task NavigatedFrom()
    {
        return base.NavigatedFrom();
    }
}
