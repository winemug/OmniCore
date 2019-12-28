using System;
using System.Collections.Generic;
using System.Text;
using OmniCore.Model.Interfaces.Platform;
using Unity;
using Xamarin.Forms;

namespace OmniCore.Client.Views.Base
{
    public class UnityRouteFactory : RouteFactory
    {
        private readonly IUnityContainer Container;
        
        public UnityRouteFactory(IUnityContainer unityContainer)
        {
            Container = unityContainer;
        }

        private Type ViewType;
        public UnityRouteFactory WithType(Type viewType)
        {
            ViewType = viewType;
            return this;
        }
        
        public override Element GetOrCreate()
        {
            return Container.Resolve(ViewType) as Element;
        }
    }
}
