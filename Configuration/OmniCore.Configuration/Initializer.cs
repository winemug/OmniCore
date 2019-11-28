using System;
using System.Collections.Generic;
using System.Text;
using OmniCore.Model.Interfaces.Services;
using Unity;

namespace OmniCore.Configuration
{
    public static class Initializer
    {
        public static IUnityContainer WithDefaultServiceProvider(this IUnityContainer container)
        {
            container.RegisterType<ICoreServices, LocalServices>();
            container.RegisterType<ICoreServicesProvider, CoreServicesProvider>();
            return container;
        }

        public static IUnityContainer WithSqlite(this IUnityContainer container)
        {
            OmniCore.Repository.Sqlite.Initializer.RegisterTypes(container);
            return container;
        }

        public static IUnityContainer WithRileyLink(this IUnityContainer container)
        {
            OmniCore.Radios.RileyLink.Initializer.RegisterTypes(container);
            return container;
        }

        public static IUnityContainer WithOmnipodEros(this IUnityContainer container)
        {
            OmniCore.Eros.Initializer.RegisterTypes(container);
            return container;
        }

    }
}
