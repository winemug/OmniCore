using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;
using System.Threading.Tasks;
using OmniCore.Client.Interfaces;
using OmniCore.Client.Views.Base;
using OmniCore.Client.Views.Testing;
using Unity;
using Xamarin.Forms;

namespace OmniCore.Client.ViewModels.Base
{
    public class ShellViewModel : BaseViewModel
    {
        private readonly IUnityContainer Container;

        public DataTemplate EmptyView => new DataTemplate(() => Container.Resolve<EmptyView>());
        public DataTemplate RadiosView => new DataTemplate(() => Container.Resolve<RadiosView>());
        public string Title => "OmniCore";

        public ShellViewModel(IUnityContainer container)
        {
            Container = container;
        }

        public override Task Initialize()
        {
            return Task.CompletedTask;
        }

        public override Task Dispose()
        {
            return Task.CompletedTask;
        }

        //private void PopulateShellItems()
        //{
        //    MainShellItems = new List<Tab>();

        //    var tab = new Tab()
        //    {
        //        FlyoutDisplayOptions = FlyoutDisplayOptions.AsSingleItem,
        //        Title = "A",
        //    };

        //    var tabContents = new List<ShellContent>
        //    {
        //        new ShellContent()
        //        {
        //            Title = "A0",
        //            Content = new DataTemplate(() => Container.Resolve<EmptyView>())
        //        },
        //        new ShellContent()
        //        {
        //            Title = "A1",
        //            Content = new DataTemplate(() => Container.Resolve<EmptyView>()),
        ////            Con
        //        },
        //        new ShellContent()
        //        {
        //            Title = "A2",
        //            Content = new DataTemplate(() => Container.Resolve<EmptyView>())
        //        }
        //    };

        //    var binding = new Binding("", source: tabContents);
        //    tab.SetBinding(Tab.ItemsProperty, binding);
        //    MainShellItems.Add(tab);
        //}
    }
}
