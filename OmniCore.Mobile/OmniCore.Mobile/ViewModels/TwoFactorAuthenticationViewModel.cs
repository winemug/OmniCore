using System;
using System.Diagnostics;
using OmniCore.Mobile.Views;
using OmniCore.Services;
using OmniCore.Services.Entities;
using Unity;
using Xamarin.Essentials;
using Xamarin.Forms;

namespace OmniCore.Mobile.ViewModels
{
    public class TwoFactorAuthenticationViewModel : BaseViewModel
    {
        public string VerificationCode { get; set; }
        public Command VerifyCommand { get; set; }

        private ChallengeRequest _challengeRequest;
        public TwoFactorAuthenticationViewModel(ChallengeRequest challengeRequest)
        {
            _challengeRequest = challengeRequest;
            VerifyCommand = new Command(VerifyClicked);
        }

        private async void VerifyClicked()
        {
            var apiClient = UnityContainer.Resolve<ApiClient>();
            var cs = UnityContainer.Resolve<ConfigurationStore>();
            var cc = await cs.GetConfigurationAsync();
            var cr = new ChallengeResponse()
            {
                RequestId = _challengeRequest.RequestId,
                VerificationCode = VerificationCode
            };
            try
            {
                cc = await apiClient.RespondToRegisterClientChallengeAsync(cc, cr);
            }
            catch (Exception e)
            {
                Debug.WriteLine(e);
                return;
            }

            await cs.SetConfigurationAsync(cc);
            await NavigationService.NavigateAsync<StartPage>();
        }
    }
}