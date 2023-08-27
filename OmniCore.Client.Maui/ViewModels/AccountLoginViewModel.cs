namespace OmniCore.Maui.ViewModels;

public class AccountLoginViewModel
{
            // if (appConfiguration.AccountEmail == null)
            // {
            //     var result = await _apiClient.PostRequestAsync<AccountRegistrationRequest, ApiResponse>(
            //         Routes.AccountRegistrationRequestRoute, new AccountRegistrationRequest
            //         {
            //             Email = email
            //         });
            //     if (result is not { Success: true })
            //         return;
            //     _appConfiguration.AccountEmail = email;
            // }
            //
            // if (!_appConfiguration.AccountVerified)
            // {
            //     var result = await _apiClient.PostRequestAsync<AccountVerificationRequest, ApiResponse>(
            //         Routes.AccountVerificationRequestRoute, new AccountVerificationRequest
            //         {
            //             Email = _appConfiguration.AccountEmail,
            //             Password = password,
            //             Code = code
            //         });
            //     if (result is not { Success: true })
            //         return;
            //
            //     _appConfiguration.AccountVerified = true;
            // }
            //
            // if (_appConfiguration.ClientAuthorization == null)
            // {
            //     var result = await _apiClient.PostRequestAsync<ClientRegistrationRequest, ClientRegistrationResponse>(
            //         Routes.ClientRegistrationRequestRoute, new ClientRegistrationRequest
            //         {
            //             Email = _appConfiguration.AccountEmail,
            //             Password = password,
            //             ClientName = clientName
            //         });
            //     if (result is not { Success: true })
            //         return;
            //
            //     _appConfiguration.ClientAuthorization = new ClientAuthorization
            //     {
            //         ClientId = result.ClientId,
            //         Token = result.Token
            //     };
            // }
}