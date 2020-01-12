using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OmniCore.Client.ViewModels.Base;
using OmniCore.Model.Interfaces.Platform.Common;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace OmniCore.Client.Views.Base
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class EmptyView : IView
    {
        public EmptyView()
        {
            InitializeComponent();
        }
    }
}