using OmniCore.Services;
using Xamarin.Forms;

namespace OmniCore.Mobile.ViewModels
{
    public class RegisterClientViewModelOld : BaseViewModelOld
    {
        public Command RegisterClientCommand { get; }
        public string Email { get; }

        public RegisterClientViewModelOld()
        {
            RegisterClientCommand = new Command(OnRegisterClientClicked);
        }

        private async void OnRegisterClientClicked(object obj)
        {
        }
    }
}