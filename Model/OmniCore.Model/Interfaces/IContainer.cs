using System.Threading.Tasks;

namespace OmniCore.Model.Interfaces
{
    public interface IContainer
    {
        IContainer Many<T>();
        IContainer Many<TI, TC>() where TC : TI;
        IContainer One<T>();
        IContainer One<TI, TC>() where TC : TI;
        IContainer One<TI, TC>(string discriminator) where TC : TI;
        IContainer Existing<T>(T instance);
        Task<T> Get<T>();
    }
}