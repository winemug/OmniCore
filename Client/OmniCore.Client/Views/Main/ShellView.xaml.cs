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
using OmniCore.Model.Interfaces.Platform;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace OmniCore.Client.Views.Main
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class ShellView : Shell, IView<ShellViewModel>
    {
        public ShellViewModel ViewModel { get; }

        public ShellView(ShellViewModel viewModel)
        {
            var task = Task.Run( async () => viewModel.OnInitialize());
            task.Wait();
            if (!task.IsCompletedSuccessfully)
            {
                throw new OmniCoreUserInterfaceException(FailureType.UserInterfaceInitialization, null, task.Exception);
            }
            BindingContext = ViewModel = viewModel;
            InitializeComponent();
        }
    }
}