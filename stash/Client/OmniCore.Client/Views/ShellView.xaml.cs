using OmniCore.Client.Views.Base;
using OmniCore.Client.Views.Home;
using OmniCore.Client.Views.Test;
using OmniCore.Model.Interfaces;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace OmniCore.Client.Views
{
    [XamlCompilation(XamlCompilationOptions.Skip)]
    public partial class ShellView : IView
    {
        public ShellView()
        {
            InitializeComponent();

            BindingContext = this;
        }
    }
}