using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using OmniCore.Mobile.ViewModels;
using Unity;
using Xamarin.Forms;

namespace OmniCore.Mobile.Services
{
    public class NavigationService
    {
        private BaseViewModel _currentModel;
        private Page _currentPage;
        private AppShell _shell;
        private readonly Dictionary<Type, Type> _typeDict = new Dictionary<Type, Type>();

        [Unity.Dependency] public IUnityContainer Container { get; set; }

        public void SetShellInstance(AppShell shell)
        {
            _shell = shell;
            _shell.Navigated += ShellOnNavigated;
            _shell.Navigating += ShellOnNavigating;
        }

        private BaseViewModel GetViewModel(Type tPage)
        {
            _typeDict.TryGetValue(tPage, out var vmType);
            if (vmType != null) return Container.Resolve(vmType, null) as BaseViewModel;
            return null;
        }

        public async Task NavigateAsync<TPage>()
        {
            await _shell.GoToAsync($"{typeof(TPage).Name}");
        }

        public void Map<TPage, TViewModel>()
        {
            Container.RegisterType<TViewModel>();
            _typeDict.Add(typeof(TPage), typeof(TViewModel));
            Routing.RegisterRoute(typeof(TPage).Name, typeof(TPage));
        }


        private async void ShellOnNavigating(object sender, ShellNavigatingEventArgs e)
        {
            Debug.WriteLine(
                $"Shell OnNavigating Source {e.Source} Current {e.Current?.Location} Target {e.Target?.Location}");
        }

        private async void ShellOnNavigated(object sender, ShellNavigatedEventArgs e)
        {
            Debug.WriteLine(
                $"Shell OnNavigated Source: {e.Source} Current: {e.Current?.Location} Previous: {e.Previous?.Location}");
            _currentPage = _shell.CurrentPage;

            if (_currentPage != null)
            {
                if (_currentModel != null)
                    await _currentModel.DisposeAsync();
                _currentModel = GetViewModel(_currentPage.GetType());
                if (_currentModel != null) await _currentModel.SetPage(_currentPage);
            }
        }

        public async Task OnResumeAsync()
        {
            // await _currentModel?.NavigatedToAsync();
        }

        public async Task OnSleepAsync()
        {
            // await _currentModel?.NavigatedAwayAsync();
        }
    }
}