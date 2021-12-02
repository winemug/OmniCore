using Xamarin.Forms;

namespace OmniCore.Services
{
    public static class Startup
    {
        public static void RegisterServices()
        {
            DependencyService.RegisterSingleton(new DataStore());
        }
    }
}