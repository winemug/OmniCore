using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Nito.AsyncEx.Synchronous;
using OmniCore.Client.Interfaces;
using OmniCore.Client.ViewModels;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace OmniCore.Client
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class ShellView : Shell, IView<ShellViewModel>
    {
        public ShellViewModel ViewModel { get; }

        public ShellView(ShellViewModel viewModel)
        {
            viewModel.Initialize().WaitAndUnwrapException();
            BindingContext = ViewModel = viewModel;
            InitializeComponent();
        }
    }
}