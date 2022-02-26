using OmniCore.Mobile.Views;
using System;
using System.Collections.Generic;
using System.Text;
using OmniCore.Services;
using Unity;
using Xamarin.Forms;

namespace OmniCore.Mobile.ViewModels
{
    public class AccountLoginViewModel : BaseViewModel
    {
        public string Email { get; set; }
        public string Password { get; set; }
        public Command LoginCommand { get; }
        public AccountLoginViewModel()
        {
            LoginCommand = new Command(OnLoginClicked);
        }

        private async void OnLoginClicked(object obj)
        {
            var apiClient = UnityContainer.Resolve<ApiClient>();
            //await apiClient.AuthorizeAccountAsync(Email, Password);
            await Shell.Current.GoToAsync($"//StartPage");
        }
    }
}
