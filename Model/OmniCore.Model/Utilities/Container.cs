using System;
using System.Linq;
using System.Threading.Tasks;
using OmniCore.Model.Interfaces;
using Unity;

namespace OmniCore.Model.Utilities
{
    public class Container : IContainer
    {
        private readonly IUnityContainer UnityContainer;
        public Container(IUnityContainer unityContainer)
        {
            UnityContainer = unityContainer;
            unityContainer.RegisterInstance(this);
        }

        public IContainer Many<T>()
        {
            UnityContainer.RegisterType<T>();
            return this;
        }

        public IContainer One<T>()
        {
            UnityContainer.RegisterSingleton<T>();
            return this;
        }

        public IContainer Existing<T>(T instance)
        {
            UnityContainer.RegisterInstance(instance);
            return this;
        }

        public IContainer Many<TI, TC>() where TC : TI
        {
            UnityContainer.RegisterType<TI, TC>();
            return this;
        }

        public IContainer One<TI, TC>() where TC : TI
        {
            UnityContainer.RegisterSingleton<TI, TC>();
            return this;
        }

        public IContainer One<TI, TC>(string discriminator) where TC : TI
        {
            UnityContainer.RegisterSingleton<TI, TC>(discriminator);
            return this;
        }

        public async Task<T> Get<T>()
        {
            var o = UnityContainer.Resolve<T>();
            var ii = o as IInitializable;
            if (ii != null)
                await ii?.Initialize();
            return o;
        }

        public async Task<T[]> GetAll<T>()
        {
            var r= Enumerable.ToArray<T>(UnityContainer
                .Registrations
                .Where(r => Enumerable.Any<Type>(r.MappedToType.GetInterfaces(), i => i == typeof(T)))
                .Select(x => (T) ((IUnityContainer) this).Resolve(x.RegisteredType, x.Name)));
            foreach (var o in r)
            {
                var ii = o as IInitializable;
                if (ii != null)
                    await ii.Initialize();
            }
            return r;
        }
    }
}