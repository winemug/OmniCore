using OmniCore.Model.Interfaces.Client;
using Xamarin.Forms.Xaml;

namespace OmniCore.Client.Views.Wizards.SetupWizard
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class SetupWizardRootView : IView
    {
        public SetupWizardRootView()
        {
            InitializeComponent();
        }
    }
}