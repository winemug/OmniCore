using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Nito.AsyncEx.Synchronous;
using OmniCore.Client.ViewModels.Base;
using OmniCore.Client.Views.Base;
using OmniCore.Client.Views.Home;
using OmniCore.Model.Enumerations;
using OmniCore.Model.Exceptions;
using OmniCore.Model.Interfaces.Common;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace OmniCore.Client.Views.Main
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class ShellView : Shell, IView<ShellViewModel>
    {
        public ShellView()
        {
            InitializeComponent();
        }

        public void SetViewModel(ShellViewModel viewModel)
        {
            BindingContext = viewModel;
        }
    }
}