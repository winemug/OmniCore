using Plugin.BLE;

namespace OmniCoreMaui;
using Plugin.BLE.Abstractions.Contracts;
using System.Diagnostics;

public partial class MainPage : ContentPage
{
    int count = 0;

    private IAdapter _adapter;
    public MainPage()
    {
        InitializeComponent();
        var ble = CrossBluetoothLE.Current;
        var adapter = CrossBluetoothLE.Current.Adapter;
        adapter.DeviceDiscovered += (s, a) => {
            Debug.WriteLine($"{a.Device.Id} discovered");
        };
        _adapter = adapter;
    }

    private async void OnCounterClicked(object sender, EventArgs e)
    {
        count++;

        if (count == 1)
            CounterBtn.Text = $"Clicked {count} time";
        else
            CounterBtn.Text = $"Clicked {count} times";

        SemanticScreenReader.Announce(CounterBtn.Text);

        await _adapter.StartScanningForDevicesAsync();

    }
}