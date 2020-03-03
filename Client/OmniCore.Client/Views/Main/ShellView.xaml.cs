using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Nito.AsyncEx.Synchronous;
using OmniCore.Client.ViewModels.Base;
using OmniCore.Client.Views.Base;
using OmniCore.Client.Views.Home;
using OmniCore.Model.Enumerations;
using OmniCore.Model.Exceptions;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;
using System.Reactive.Linq;
using OmniCore.Model.Interfaces.Client;
using Rg.Plugins.Popup.Services;

namespace OmniCore.Client.Views.Main
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class ShellView : Shell
    {
        public DataTemplate EmptyView { get; }
        public DataTemplate RadiosView { get; }
        public DataTemplate ActivePodsView { get; }
        public DataTemplate WaitingPodsView { get; }
        public DataTemplate ArchivedPodsView { get; }

        public ShellView(IViewPresenter viewPresenter)
        {
            InitializeComponent();

            EmptyView = new DataTemplate(() => viewPresenter.GetView<EmptyView>(true));
            RadiosView = new DataTemplate(() => viewPresenter.GetView<RadiosView>(true));
            ActivePodsView = new DataTemplate(() => viewPresenter.GetView<ActivePodsView>(true));
            WaitingPodsView = new DataTemplate(() => viewPresenter.GetView<EmptyView>(true));
            ArchivedPodsView = new DataTemplate(() => viewPresenter.GetView<EmptyView>(true));
            BindingContext = this;
        }
    }
}