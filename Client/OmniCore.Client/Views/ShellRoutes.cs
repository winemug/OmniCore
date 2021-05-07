using System;
using System.Collections.Generic;
using OmniCore.Client.Views.Home;

namespace OmniCore.Client.Views.Main
{
    public static class ShellRoutes
    {
        public static readonly string RadioDetailView = "Home//Radios//RadioDetails";

        private static Dictionary<Type, string> RouteDictionary = new Dictionary<Type, string>
        {
            {typeof(RadioDetailView), RadioDetailView}
        };

        //public static void RegisterRoutes(IUnityContainer container)
        //{
        //    foreach (var entry in RouteDictionary)
        //    {
        //        var urf = container.Resolve<UnityRouteFactory>().WithType(entry.Key);
        //        Routing.RegisterRoute(entry.Value, urf);
        //    }
        //}
    }
}