using OmniCore.Client.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace OmniCore.Client.Views
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class MainPage : Shell, IView
    {
        public MainPage()
        {
            InitializeComponent();
            BindingContext = this;
        }

        public object[] Pods = new object[10];
    }
}