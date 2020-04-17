using System.Linq;
using OmniCore.Model.Interfaces.Common;
using Unity;

namespace OmniCore.Services
{
    public class Container<R> : UnityContainer, IContainer<R>
        where R : IInstance
    {
        public Container()
        {
            this.RegisterInstance<IContainer<R>>(this);
        }

        public IContainer<R> Many<T>()
            where T : R
        {
            this.RegisterType<T>();
            return this;
        }

        public IContainer<R> One<T>()
            where T : R
        {
            this.RegisterSingleton<T>();
            return this;
        }

        public IContainer<R> Existing<T>(T instance)
            where T : R
        {
            this.RegisterInstance(instance);
            return this;
        }

        public IContainer<R> Many<TI, TC>() where TC : TI
            where TI : R
        {
            this.RegisterType<TI, TC>();
            return this;
        }

        public IContainer<R> One<TI, TC>() where TC : TI
            where TI : R
        {
            this.RegisterSingleton<TI, TC>();
            return this;
        }

        public IContainer<R> One<TI, TC>(string discriminator) where TC : TI
            where TI : R
        {
            this.RegisterSingleton<TI, TC>(discriminator);
            return this;
        }

        public T Get<T>()
            where T : R
        {
            return this.Resolve<T>();
        }

        public T[] GetAll<T>()
            where T : R
        {
            return Registrations
                .Where(r => r.MappedToType.GetInterfaces()
                    .Any(i => i == typeof(T)))
                .Select(x => (T) ((IUnityContainer) this).Resolve(x.RegisteredType, x.Name))
                .ToArray();
        }
    }
}