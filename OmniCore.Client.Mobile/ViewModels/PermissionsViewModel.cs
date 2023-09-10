﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using OmniCore.Client.Interfaces.Services;
using OmniCore.Client.Mobile.Services;

namespace OmniCore.Client.Mobile.ViewModels;

public partial class PermissionsViewModel : ObservableObject
{
    private readonly IPlatformPermissionService _platformPermissionService;
    [ObservableProperty] private string? _permissionName;

    [ObservableProperty] private string? _permissionResult;

    public PermissionsViewModel(IPlatformPermissionService platformPermissionService)
    {
        _platformPermissionService = platformPermissionService;
    }

    [RelayCommand]
    private async void CheckPermission()
    {
        try
        {
            var r = await _platformPermissionService.GetPermissionStatusAsync("");
            PermissionResult = r.ToString();
        }
        catch (Exception e)
        {
            PermissionResult = e.Message;
        }
    }

}
