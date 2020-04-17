using OmniCore.Model.Interfaces.Client;
using Xamarin.Forms.Xaml;

namespace OmniCore.Client.Views.Wizards.NewPod
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class PodWizardMainView : IView
    {
        public PodWizardMainView()
        {
            InitializeComponent();
        }
    }
}