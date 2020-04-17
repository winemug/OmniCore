using System;
using OmniCore.Model.Interfaces.Common;
using Xamarin.Forms;

namespace OmniCore.Client.Views.Base
{
    public class UnityRouteFactory : RouteFactory, IClientInstance
    {
        private readonly IContainer<IClientInstance> Container;

        private Type ViewType;

        public UnityRouteFactory(IContainer<IClientInstance> clientContainer)
        {
            Container = clientContainer;
        }

        public UnityRouteFactory WithType(Type viewType)
        {
            ViewType = viewType;
            return this;
        }

        public override Element GetOrCreate()
        {
            return null;
        }
    }
}