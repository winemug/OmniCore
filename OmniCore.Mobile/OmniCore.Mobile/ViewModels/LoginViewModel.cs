using OmniCore.Mobile.Views;
using System;
using System.Collections.Generic;
using System.Text;
using OmniCore.Services;
using Xamarin.Forms;

namespace OmniCore.Mobile.ViewModels
{
    public class LoginViewModel : BaseViewModel
    {
        public string Email { get; set; }
        
        public string Password { get; set; }
        public Command LoginCommand { get; }
        public LoginViewModel(Page page): base(page)
        {
            LoginCommand = new Command(OnLoginClicked);
        }

        private async void OnLoginClicked(object obj)
        {
            // Prefixing with `//` switches to a different navigation stack instead of pushing to the active one
            await Shell.Current.GoToAsync($"//{nameof(AboutPage)}");
        }
    }
}
