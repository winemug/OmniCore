using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using OmniCore.Model.Interfaces;
using OmniCore.Model.Interfaces.Platform;
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
            this.RegisterType<TI,TC>();
            return this;
        }

        public ICoreContainer One<TI, TC>() where TC : TI
        {
            this.RegisterSingleton<TI, TC>();
            return this;
        }

        public T Get<T>()
        {
            return this.Resolve<T>();
        }

        public T[] GetAll<T>()
        {
            return this.Registrations
                .Where(r => r.MappedToType.GetInterfaces()
                    .Any(i => i == typeof(T)))
                .Select(x => (T) ((IUnityContainer) this).Resolve(x.RegisteredType, x.Name))
                .ToArray();
        }
    }
}
