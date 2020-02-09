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

namespace OmniCore.Client.Views.Main
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class ShellView : Shell
    {
        public DataTemplate EmptyView { get; }
        public DataTemplate RadiosView { get; }
        public DataTemplate PodsView { get; }

        public ShellView(IViewPresenter viewPresenter)
        {
            InitializeComponent();
            EmptyView = new DataTemplate(() => viewPresenter.GetView<EmptyView>(true));
            RadiosView = new DataTemplate(() => viewPresenter.GetView<RadiosView>(true));
            PodsView = new DataTemplate(() => viewPresenter.GetView<PodsView>(true));

            BindingContext = this;
        }
    }
}