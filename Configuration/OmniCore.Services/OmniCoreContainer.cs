using System;
using System.Collections.Generic;
using System.Linq;
using OmniCore.Model.Interfaces;
using OmniCore.Model.Interfaces.Services;
using Unity;

namespace OmniCore.Services
{
    public class OmniCoreContainer : UnityContainer, ICoreContainer
    {
        public OmniCoreContainer()
        {
            this.RegisterInstance<ICoreContainer>(this);
        }

        public ICoreContainer Many<T>()
        {
            this.RegisterType<T>();
            return this;
        }

        public ICoreContainer One<T>()
        {
            this.RegisterSingleton<T>();
            return this;
        }

        public ICoreContainer Existing<T>(T instance)
        {
            this.RegisterInstance(instance);
            return this;
        }

        public ICoreContainer Many<TI, TC>() where TC : TI
        {
            throw new System.NotImplementedException();
        }

        public ICoreContainer One<TI, TC>() where TC : TI
        {
            this.RegisterSingleton<TI, TC>();
            return this;
        }

        public IList<T> AllAssignable<T>()
        {
            return this.Registrations
                .Where(r => r.MappedToType.IsAssignableFrom(typeof(T)))
                .Select(x => (T) ((IUnityContainer)this).Resolve(x.RegisteredType, x.Name))
                .ToList();
        }

        public T Get<T>()
        {
            return this.Resolve<T>();
        }
    }
}
