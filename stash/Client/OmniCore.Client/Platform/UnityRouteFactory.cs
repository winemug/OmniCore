using System;
using OmniCore.Model.Interfaces;
using Xamarin.Forms;

namespace OmniCore.Client.Views.Base
{
    public class UnityRouteFactory : RouteFactory
    {
        private readonly IContainer Container;

        private Type ViewType;

        public UnityRouteFactory(IContainer clientContainer)
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