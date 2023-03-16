using System.Diagnostics;
using OmniCore.Maui.Services;
using OmniCore.Maui.ViewModels;

namespace OmniCore.Maui;

public partial class AppShell : Shell
{
    private ViewModelViewMapper _mapper;
    private BaseViewModel _activeViewModel;
    public AppShell(ViewModelViewMapper mapper)
    {
        _mapper = mapper;
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
        _activeViewModel = _mapper.GetViewModel(CurrentPage.GetType(), Handler.MauiContext.Services);
        await _activeViewModel.OnAppearing();
        CurrentPage.BindingContext = _activeViewModel;
    }
}