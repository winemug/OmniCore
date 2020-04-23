using OmniCore.Model.Interfaces;
using OmniCore.Model.Interfaces.Services.Internal;

namespace OmniCore.Repository
{
    public static class Initializer
    {
        public static IContainer WithEfCoreRepository
            (this IContainer container)
        {
            return container
                .Many<IRepositoryContextReadOnly, RepositoryContext>()
                .Many<IRepositoryContextReadWrite, RepositoryContext>();
        }
    }
}