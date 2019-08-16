using OmniCore.Mobile.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace OmniCore.Mobile.Views
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class MainPage : Shell, IView
    {
        public MainPage()
        {
            InitializeComponent();
            //MainShell.Items
        }
    }
}