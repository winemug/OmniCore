using System;
using System.Collections.Generic;
using System.Text;
using OmniCore.Model.Interfaces.Data;
using OmniCore.Model.Interfaces.Platform.Common;

namespace OmniCore.Repository
{
    public static class Initializer
    {
        public static ICoreContainer<IServerResolvable> WithEfCoreRepository
            (this ICoreContainer<IServerResolvable> container)
        {
            return container
                .One<IRepositoryService, RepositoryService>()
                .Many<IRepositoryContext, RepositoryContext>();
        }
    }
}
