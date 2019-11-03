using OmniCore.Client.Views.Help;
using OmniCore.Client.Views.Pods;
using OmniCore.Client.Views.Settings;
using OmniCore.Client.Views.Test;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics.CodeAnalysis;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Forms;

namespace OmniCore.Client.ViewModels
{
    public class MainViewModel : PageViewModel
    {
        public MainViewModel(Page page) : base(page)
        {
        }

        public ObservableCollection<FlyoutViewModel> ShellItems { get; private set; }

        [method: SuppressMessage("", "CS1998", Justification = "Not applicable")]
        protected async override Task<BaseViewModel> BindData()
        {
            ShellItems = GetShellItems();
            return this;
        }

        protected override void OnDisposeManagedResources()
        {
            if (ShellItems != null)
            {
                foreach (IDisposable item in ShellItems)
                    item.Dispose();
            }
        }

        private ObservableCollection<FlyoutViewModel> GetShellItems()
        {
            var items = new ObservableCollection<FlyoutViewModel>()
            {
                new FlyoutViewModel()
                {
                    Title = "Pod",
                    Tabs = new ObservableCollection<TabViewModel>()
                                        {
                                            new TabViewModel()
                                            {
                                                Title = "Overview",
                                                Content = new OverviewPage()
                                            },
                                        }
                },
                new FlyoutViewModel()
                {
                    Title = "Settings",
                    Tabs = new ObservableCollection<TabViewModel>()
                                        {
                                            new TabViewModel()
                                            {
                                                Title = "General",
                                                Content = new GeneralSettingsPage()
                                            },
                                        }
                },
#if DEBUG
                new FlyoutViewModel()
                {
                    Title = "Test",
                    Tabs = new ObservableCollection<TabViewModel>()
                                        {
                                            new TabViewModel()
                                            {
                                                Title = "Debug",
                                                Content = new DebugPage()
                                            },
                                        }
                },
#endif
                new FlyoutViewModel()
                {
                    Title = "Help",
                    Tabs = new ObservableCollection<TabViewModel>()
                                        {
                                            new TabViewModel()
                                            {
                                                Title = "About",
                                                Content = new AboutPage()
                                            },
                                        }
                }
            };
            return items;
        }
    }
}
