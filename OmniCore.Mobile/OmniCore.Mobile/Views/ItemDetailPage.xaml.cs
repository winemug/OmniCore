using OmniCore.Mobile.ViewModels;
using System.ComponentModel;
using Xamarin.Forms;

namespace OmniCore.Mobile.Views
{
    public partial class ItemDetailPage : ContentPage
    {
        public ItemDetailPage()
        {
            InitializeComponent();
            BindingContext = new ItemDetailViewModelOld();
        }
    }
}