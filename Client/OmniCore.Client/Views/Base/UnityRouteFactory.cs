using System;
using System.Collections.Generic;
using System.Text;
using OmniCore.Model.Interfaces;
using OmniCore.Model.Interfaces.Platform;
using Xamarin.Forms;

namespace OmniCore.Client.Views.Base
{
    public class UnityRouteFactory : RouteFactory
    {
        private readonly ICoreContainer Container;
        
        public UnityRouteFactory(ICoreServices services)
        {
            Container = services.Container;
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
