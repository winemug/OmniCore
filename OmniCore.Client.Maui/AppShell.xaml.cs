using System.Diagnostics;
using OmniCore.Maui.Services;
using OmniCore.Maui.ViewModels;

namespace OmniCore.Maui;

public partial class AppShell : Shell
{
    public AppShell()
    {
        InitializeComponent();
    }
    // protected override async void OnNavigating(ShellNavigatingEventArgs args)
    // {
    //     Debug.WriteLine($"Shell.OnNavigating");
    //
    //     if (_activeViewModel != null)
    //     {
    //         await _activeViewModel.OnDisappearing();
    //         await _activeViewModel.DisposeAsync();
    //         _activeViewModel = null;
    //     }
    // }
    // protected override async void OnNavigated(ShellNavigatedEventArgs args)
    // {
    //     Debug.WriteLine($"Shell.OnNavigated");
    //
    //
    //     _activeViewModel = _serviceProvider.GetViewModel(CurrentPage.GetType());
    //     await _activeViewModel.OnAppearing();
    //     CurrentPage.BindingContext = _activeViewModel;
    // }
    //
    // private void CurrentPageOnDisappearing(object? sender, EventArgs e)
    // {
    //     Debug.WriteLine($"Page.OnAppearing");
    // }
    //
    // private void CurrentPageOnAppearing(object? sender, EventArgs e)
    // {
    //     Debug.WriteLine($"Page.OnDisappearing");
    // }
}