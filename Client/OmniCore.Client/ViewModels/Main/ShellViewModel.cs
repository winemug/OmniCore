using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;
using System.Threading.Tasks;
using Nito.AsyncEx;
using OmniCore.Client.ViewModels.Home;
using OmniCore.Client.Views.Base;
using OmniCore.Client.Views.Home;
using OmniCore.Model.Interfaces.Platform;
using Xamarin.Forms;

namespace OmniCore.Client.ViewModels.Base
{
    public class ShellViewModel : BaseViewModel
    {
        public DataTemplate EmptyView => new DataTemplate(() => Client.GetView<EmptyView, EmptyViewModel>());
        public DataTemplate RadiosView => new DataTemplate(() => Client.GetView<RadiosView, RadiosViewModel>());
        public DataTemplate PodsView => new DataTemplate(() => Client.GetView<PodsView, PodsViewModel>());
        public string Title => "OmniCore";

        public ShellViewModel(ICoreClient client) : base(client)
        {
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
        protected override Task OnInitialize()
        {
            return Task.CompletedTask;
        }
    }
}
