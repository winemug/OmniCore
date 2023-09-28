using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using OmniCore.Client.Interfaces.Services;
using OmniCore.Client.Mobile.Services;

namespace OmniCore.Client.Mobile.ViewModels;

public partial class ListRadiosViewModel : ViewModel
{
    private readonly IBleService _bleService;

    public ListRadiosViewModel(IBleService bleService)
    {
        _bleService = bleService;
        _bleDevices = bleService.BleDeviceList;
    }

    [ObservableProperty] private ObservableCollection<IBleDevice> _bleDevices;

    [RelayCommand]
    private async Task StartSearchAsync()
    {
        await _bleService.StartSearchAsync(CancellationToken.None);
    }
    
    [RelayCommand]
    private async Task StopSearchAsync()
    {
        await _bleService.StopSearchAsync(CancellationToken.None);
    }

}