using System.Linq;
using System.Threading.Tasks;
using Nito.AsyncEx.Synchronous;
using OmniCore.Model.Interfaces.Common;
using Unity;

namespace OmniCore.Services
{
    public class Container : UnityContainer, IContainer
    {
        public Container()
        {
            this.RegisterInstance<IContainer>(this);
        }

        public IContainer Many<T>()
        {
            this.RegisterType<T>();
            return this;
        }

        public IContainer One<T>()
        {
            this.RegisterSingleton<T>();
            return this;
        }

        public IContainer Existing<T>(T instance)
        {
            this.RegisterInstance(instance);
            return this;
        }

        public IContainer Many<TI, TC>() where TC : TI
        {
            this.RegisterType<TI, TC>();
            return this;
        }

        public IContainer One<TI, TC>() where TC : TI
        {
            this.RegisterSingleton<TI, TC>();
            return this;
        }

        public IContainer One<TI, TC>(string discriminator) where TC : TI
        {
            this.RegisterSingleton<TI, TC>(discriminator);
            return this;
        }

        public async Task<T> Get<T>()
        {
            var o = this.Resolve<T>();
            var ii = o as IInitializable;
            if (ii != null)
                await ii?.Initialize();
            return o;
        }

        public async Task<T[]> GetAll<T>()
        {
            var r= Registrations
                .Where(r => r.MappedToType.GetInterfaces()
                    .Any(i => i == typeof(T)))
                .Select(x => (T) ((IUnityContainer) this).Resolve(x.RegisteredType, x.Name))
                .ToArray();
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