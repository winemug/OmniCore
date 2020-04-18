using System.Threading.Tasks;

namespace OmniCore.Model.Interfaces.Common
{
    public interface IContainer<in TInstance> : IServiceInstance, IClientInstance
        where TInstance : IInstance
    {
        IContainer<TInstance> Many<T>() where T : TInstance;
        IContainer<TInstance> Many<TI, TC>() where TC : TI where TI : TInstance;
        IContainer<TInstance> One<T>() where T : TInstance;
        IContainer<TInstance> One<TI, TC>() where TC : TI where TI : TInstance;
        IContainer<TInstance> One<TI, TC>(string discriminator) where TC : TI where TI : TInstance;
        IContainer<TInstance> Existing<T>(T instance) where T : TInstance;
        Task<T> Get<T>() where T : TInstance;
    }
}