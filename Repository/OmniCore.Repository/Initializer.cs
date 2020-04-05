using OmniCore.Model.Interfaces.Common;
using OmniCore.Model.Interfaces.Services.Internal;

namespace OmniCore.Repository
{
    public static class Initializer
    {
        public static ICoreContainer<IServerResolvable> WithEfCoreRepository
            (this ICoreContainer<IServerResolvable> container)
        {
            return container
                .Many<IRepositoryContextReadOnly, RepositoryContext>()
                .Many<IRepositoryContextReadWrite, RepositoryContext>();
        }
    }
}