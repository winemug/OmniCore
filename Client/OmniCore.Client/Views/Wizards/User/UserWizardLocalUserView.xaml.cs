using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OmniCore.Model.Interfaces;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace OmniCore.Client.Views.Wizards.User
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class UserWizardLocalUserView : IView
    {
        public UserWizardLocalUserView()
        {
            InitializeComponent();
        }
    }
}