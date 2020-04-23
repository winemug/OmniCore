using OmniCore.Model.Interfaces;
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