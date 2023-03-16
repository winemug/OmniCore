//namespace OmniCore.Mobile.ViewModels
//{
//    public class ClientRegistrationViewModel : BaseViewModel
//    {
//        // public string Name { get; set; }
//        // public Command RegisterCommand { get; }
//        // public ClientRegistrationViewModel()
//        // {
//        //     RegisterCommand = new Command(OnRegisterClicked);
//        //     Name = DeviceInfo.Name;
//        // }
//        //
//        // private async void OnRegisterClicked(object obj)
//        // {
//        //     var apiClient = UnityContainer.Resolve<ApiClient>();
//        //     var cs = UnityContainer.Resolve<ConfigurationStore>();
//        //     var cc = await cs.GetConfigurationAsync();
//        //     cc.Name = Name;
//        //     try
//        //     {
//        //         await apiClient.RegisterClientAsync(cc);
//        //     }
//        //     catch (Exception e)
//        //     {
//        //         Debug.WriteLine(e);
//        //         return;
//        //     }
//        //
//        //     await cs.SetConfigurationAsync(cc);
//        //     await NavigationService.NavigateAsync<StartPage>();
//        // }
//    }
//}