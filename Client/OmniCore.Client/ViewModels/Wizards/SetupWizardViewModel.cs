using System;
using System.Collections.Generic;
using System.Text;
using OmniCore.Client.ViewModels.Base;
using OmniCore.Client.Views.Wizards.SetupWizard;
using OmniCore.Model.Interfaces.Services;
using Xamarin.Forms;

namespace OmniCore.Client.ViewModels.Wizards
{
    public class SetupWizardViewModel : NavigationViewModel
    {
        public SetupWizardViewModel(ICoreBootstrapper bootstrapper) : base(bootstrapper)
        {
        }
    }
}
