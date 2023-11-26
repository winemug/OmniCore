﻿using System.Collections.ObjectModel;

namespace OmniCore.Client.Interfaces.Services;

public interface IBleService
{
    ObservableCollection<IBleDevice> BleDeviceList { get; }
    Task StartSearchAsync(CancellationToken cancellationToken);
    Task StopSearchAsync(CancellationToken cancellationToken);
}