using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using OmniCore.Maui.Models;
using OmniCore.Maui.Services;

namespace OmniCore.Maui.PageModels;

public partial class RadiosPageModel(BleService bleService) : PageModel
{
    [ObservableProperty] private ObservableCollection<BleDeviceModel> _radios = new ObservableCollection<BleDeviceModel>();

    protected override async Task Appearing()
    {
        await bleService.Search();
        // Radios.Clear();
        // Radios.Add(
        //     new BleDeviceModel(Guid.NewGuid(), "radio1")
        // );
    }
}
