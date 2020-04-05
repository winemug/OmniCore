using System;
using OmniCore.Model.Interfaces.Common;
using Xamarin.Forms;

namespace OmniCore.Client.Views.Base
{
    public class UnityRouteFactory : RouteFactory, IClientResolvable
    {
        private readonly ICoreContainer<IClientResolvable> Container;

        private Type ViewType;

        public UnityRouteFactory(ICoreContainer<IClientResolvable> clientContainer)
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