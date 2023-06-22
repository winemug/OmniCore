using System.Diagnostics;
using OmniCore.Maui.Services;
using OmniCore.Maui.ViewModels;

namespace OmniCore.Maui;

public partial class AppShell : Shell
{
    private BaseViewModel _activeViewModel;
    private IServiceProvider _serviceProvider;
    public AppShell(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
        InitializeComponent();
    }
    protected override async void OnNavigating(ShellNavigatingEventArgs args)
    {
        if (_activeViewModel != null)
        {
            await _activeViewModel.OnDisappearing();
            await _activeViewModel.DisposeAsync();
            _activeViewModel = null;
        }
    }
    protected override async void OnNavigated(ShellNavigatedEventArgs args)
    {
        Debug.WriteLine($"OnNavigated");

        _activeViewModel = _serviceProvider.GetViewModel(CurrentPage.GetType());
        await _activeViewModel.OnAppearing();
        CurrentPage.BindingContext = _activeViewModel;
    }
}