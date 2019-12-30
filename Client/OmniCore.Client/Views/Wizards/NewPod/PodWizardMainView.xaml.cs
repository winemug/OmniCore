using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OmniCore.Client.ViewModels.Wizards;
using OmniCore.Client.Views.Base;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace OmniCore.Client.Views.Wizards.NewPod
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class PodWizardMainView
    {
        public PodWizardMainView(PodWizardViewModel viewModel) : base(viewModel)
        {
            InitializeComponent();
        }
    }
}