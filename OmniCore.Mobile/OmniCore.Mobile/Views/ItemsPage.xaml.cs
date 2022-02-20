using OmniCore.Mobile.Models;
using OmniCore.Mobile.ViewModels;
using OmniCore.Mobile.Views;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace OmniCore.Mobile.Views
{
    public partial class ItemsPage : ContentPage
    {
        ItemsViewModelOld _viewModelOld;

        public ItemsPage()
        {
            InitializeComponent();

            BindingContext = _viewModelOld = new ItemsViewModelOld();
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();
            _viewModelOld.OnAppearing();
        }
    }
}