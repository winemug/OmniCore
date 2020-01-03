using System;
using System.Collections.Generic;
using System.Text;
using OmniCore.Model.Interfaces;
using OmniCore.Model.Interfaces.Platform;
using Xamarin.Forms;

namespace OmniCore.Client.Views.Base
{
    public class UnityRouteFactory : RouteFactory, IClientResolvable
    {
        private readonly ICoreContainer<IClientResolvable> Container;
        
        public UnityRouteFactory(ICoreContainer<IClientResolvable> clientContainer)
        {
            Container = clientContainer;
        }

        private Type ViewType;
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
