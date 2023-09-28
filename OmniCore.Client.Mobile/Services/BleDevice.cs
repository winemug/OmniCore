using System.ComponentModel;
using System.ComponentModel.Design.Serialization;
using System.Runtime.CompilerServices;
using CommunityToolkit.Mvvm.ComponentModel;
using OmniCore.Client.Interfaces.Services;
using Plugin.BLE.Abstractions.Extensions;

namespace OmniCore.Client.Mobile.Services;

public partial class BleDevice : ObservableObject, IBleDevice
{
    public Guid Address { get; set; }

    public string AddressText
    {
        get => Address.ToByteArray()[10..16].ToHexString();
    }

    [ObservableProperty] private string? _name;
    [ObservableProperty] private int _lastRssi;
    [ObservableProperty] private DateTimeOffset _lastSeen;

    [ObservableProperty] private double? _frequencyAverage;

    //private const int OffsetCount = 15;
    //private DateTimeOffset[] _offsets = new DateTimeOffset[OffsetCount];
    //private int _offsetIndex = 0;
    //partial void OnLastSeenChanged(DateTimeOffset value)
    //{
    //    _offsets[_offsetIndex] = value;
    //    if (_offsetIndex == OffsetCount - 1)
    //    {
    //        _offsetIndex = 0;
    //        FrequencyAverage = _offsets.Skip(1).Zip(_offsets.Take(OffsetCount - 1))
    //            .Average(tuple => (tuple.First - tuple.Second).TotalSeconds);
    //    }
    //    else
    //        _offsetIndex++;
    //}

    public override string ToString()
    {
        return $"{FrequencyAverage:F3}\t{Address.ToByteArray()[10..16].ToHexString()}\t{LastRssi}\t{Name}";
    }
}