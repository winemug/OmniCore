using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using OmniCore.Client.Interfaces.Services;
using OmniCore.Client.Mobile.Views;

namespace OmniCore.Client.Mobile.ViewModels;
public partial class AccountLoginViewModel : ViewModel
{
    private readonly INavigationService _navigationService;
    private readonly ICoreService _coreService;

    public AccountLoginViewModel(INavigationService  navigationService,
        ICoreService coreService)
    {
        _navigationService = navigationService;
        _coreService = coreService;
    }

    [RelayCommand]
    private async Task ContinueAsync()
    {
        using (var _ = _coreService.ForegroundActivityContext())
        {
            await Task.Delay(7500);
        }

        await _coreService.InteractiveStartAsync();
    }
}
