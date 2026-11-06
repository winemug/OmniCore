using System.Diagnostics;
using Plugin.BLE.Abstractions;
using Plugin.BLE.Abstractions.Contracts;

namespace OmniCore.Maui.Services;

public class TestRunService(BleService bleService, IBluetoothLE ble)
{
    public void StartTests()
    {
        _ = TestTask();
    }
    
    private async Task TestTask()
    {
        bleService.StartScanning();
        await Task.Delay(35000);
        Debug.WriteLine("Should be finished by now");

    }
    
}