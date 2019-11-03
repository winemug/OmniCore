using OmniCore.Model.Interfaces;
using Unity;

namespace OmniCore.Therapy
{
    public static class Initializer
    {
        public static void RegisterTypes(IUnityContainer container)
        {
            container.RegisterType<IAbsorptionProfile, Profiles.Insulin.Rapid.AbsorptionProfile>();
            container.RegisterType<ICirculationProfile, Profiles.Insulin.Rapid.CirculationProfile>();
            container.RegisterType<IDegradationProfile, Profiles.Insulin.Rapid.DegradationProfile>();

            container.RegisterType<IAbsorptionProfile, Profiles.Insulin.UltraRapid.AbsorptionProfile>();
            container.RegisterType<ICirculationProfile, Profiles.Insulin.UltraRapid.CirculationProfile>();
            container.RegisterType<IDegradationProfile, Profiles.Insulin.UltraRapid.DegradationProfile>();
        }
    }
}
