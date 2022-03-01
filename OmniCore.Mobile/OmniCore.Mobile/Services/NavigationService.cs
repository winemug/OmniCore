using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using OmniCore.Mobile.ViewModels;
using Xamarin.Essentials;
using Xamarin.Forms;

namespace OmniCore.Mobile.Services
{
    public class NavigationService
    {
        private Dictionary<Type, Type> _typeDict = new Dictionary<Type, Type>();
        private Dictionary<Type, BaseViewModel> _instanceDict = new Dictionary<Type, BaseViewModel>();
        private AppShell _shell = null;
        private Page _currentPage = null;
        private BaseViewModel _currentModel = null;
        private BaseViewModel _nextModel = null;

        public NavigationService(AppShell shell)
        {
            _shell = shell;
            _shell.Navigated += ShellOnNavigated;
            _shell.Navigating += ShellOnNavigating;
        }

        private BaseViewModel GetViewModelByPageType(Type pageType)
        {
            _instanceDict.TryGetValue(pageType, out var vmInstance);
            if (vmInstance == null)
            {
                _typeDict.TryGetValue(pageType, out var vmType);
                if (vmType != null)
                {
                    vmInstance = Activator.CreateInstance(vmType) as BaseViewModel;
                    _instanceDict.Add(pageType, vmInstance);
                }
            }
            return vmInstance;            
        }
        
        public async Task NavigateAsync<TPage>(BaseViewModel viewModel = null)
        {
            var pageType = typeof(TPage);
            if (viewModel == null)
            {
                viewModel = GetViewModelByPageType(pageType);
            }
            else
            {
                _instanceDict.TryGetValue(pageType, out var oldInstance);
                if (oldInstance != null)
                {
                    oldInstance.Dispose();
                    _instanceDict.Remove(pageType);
                }
            }

            _nextModel = viewModel;
            await MainThread.InvokeOnMainThreadAsync(async () =>
            {
                await _shell.GoToAsync(typeof(TPage).Name);
            });

        }
        public void Register<TPage, TViewModel>()
        {
            _typeDict.Add(typeof(TPage), typeof(TViewModel));
            Routing.RegisterRoute(typeof(TPage).Name, typeof(TPage));
        }

        private async void ShellOnNavigating(object sender, ShellNavigatingEventArgs e)
        {
            Debug.WriteLine($"*** NAVIGATING from {e.Source} to {e.Target}, current {e.Current}");

            if (_currentModel != null)
            {
                await _currentModel.NavigatedAwayAsync();
            }

            _currentModel = null;
            _currentPage = null;
        }

        private async void ShellOnNavigated(object sender, ShellNavigatedEventArgs e)
        {
            Debug.WriteLine($"*** NAVIGATED to {e.Current} from {e.Previous} Source: {e.Source}");
            _currentPage = _shell.CurrentPage;
            _currentModel = _nextModel;
            _nextModel = null;
            if (_currentPage != null)
            {
                if (_currentModel == null)
                    _currentModel = GetViewModelByPageType(_currentPage.GetType());

                if (_currentModel != null)
                {
                    await _currentModel.BindToPageAsync(_currentPage);
                    await _currentModel.NavigatedToAsync();
                }
            }
        }

        public async Task OnResumeAsync()
        {
        }

        public async Task OnSleepAsync()
        {
        }
    }
}