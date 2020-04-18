using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OmniCore.Model.Interfaces.Client;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace OmniCore.Client.Views.Wizards.Permissions
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class PermissionsWizardRootView : IView
    {
        public PermissionsWizardRootView()
        {
            InitializeComponent();
        }
    }
}