using OmniCore.Model.Interfaces.Common;
using OmniCore.Model.Interfaces.Services.Internal;

namespace OmniCore.Repository
{
    public static class Initializer
    {
        public static IContainer<IServiceInstance> WithEfCoreRepository
            (this IContainer<IServiceInstance> container)
        {
            return container
                .Many<IRepositoryContextReadOnly, RepositoryContext>()
                .Many<IRepositoryContextReadWrite, RepositoryContext>();
        }
    }
}