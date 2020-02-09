using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OmniCore.Model.Interfaces.Client;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace OmniCore.Client.Views.Main
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class NavigationView : IView
    {
        public NavigationView()
        {
            InitializeComponent();
        }
    }
}