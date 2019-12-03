using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;
using System.Threading.Tasks;
using OmniCore.Client.Interfaces;
using OmniCore.Client.Views;
using Unity;
using Xamarin.Forms;

namespace OmniCore.Client.ViewModels
{
    public class ShellViewModel : BaseViewModel
    {
        private readonly IUnityContainer Container;

        public DataTemplate View1 => new DataTemplate(() => Container.Resolve<EmptyView>());
        public DataTemplate View2 => new DataTemplate(() => Container.Resolve<EmptyView>());
        public DataTemplate View3 => new DataTemplate(() => Container.Resolve<EmptyView>());
        public DataTemplate View4 => new DataTemplate(() => Container.Resolve<EmptyView>());
        public DataTemplate View5 => new DataTemplate(() => Container.Resolve<EmptyView>());
        public DataTemplate View6 => new DataTemplate(() => Container.Resolve<EmptyView>());

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
    }
}
