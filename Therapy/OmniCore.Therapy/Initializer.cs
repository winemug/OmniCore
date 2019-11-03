using OmniCore.Model.Interfaces;
using Unity;

namespace OmniCore.Therapy
{
    public static class Initializer
    {
        public static void RegisterTypes(IUnityContainer container)
        {
            container.RegisterType<IAbsorptionProfile, Profiles.Insulin.Rapid.AbsorptionProfile>("NRAP1");
            container.RegisterType<ICirculationProfile, Profiles.Insulin.Rapid.CirculationProfile>("NRAP1");
            container.RegisterType<IDegradationProfile, Profiles.Insulin.Rapid.DegradationProfile>("NRAP1");

            container.RegisterType<IAbsorptionProfile, Profiles.Insulin.UltraRapid.AbsorptionProfile>("URAP1");
            container.RegisterType<ICirculationProfile, Profiles.Insulin.UltraRapid.CirculationProfile>("URAP1");
            container.RegisterType<IDegradationProfile, Profiles.Insulin.UltraRapid.DegradationProfile>("URAP1");
        }
    }
}
