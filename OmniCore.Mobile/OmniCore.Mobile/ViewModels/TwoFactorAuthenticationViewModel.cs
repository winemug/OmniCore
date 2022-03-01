using Xamarin.Forms;

namespace OmniCore.Mobile.ViewModels
{
    public class TwoFactorAuthenticationViewModel : BaseViewModel
    {
        public string VerificationCode { get; set; }
        public Command VerifyCommand { get; set; }

        public TwoFactorAuthenticationViewModel()
        {
            VerifyCommand = new Command(VerifyClicked);
        }

        private async void VerifyClicked()
        {
        }
    }
}