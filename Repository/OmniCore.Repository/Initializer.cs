using System;
using System.Collections.Generic;
using System.Text;
using OmniCore.Model.Interfaces.Common;
using OmniCore.Model.Interfaces.Services;
using OmniCore.Model.Interfaces.Services.Internal;
using OmniCore.Services;

namespace OmniCore.Repository
{
    public static class Initializer
    {
        public static ICoreContainer<IServerResolvable> WithEfCoreRepository
            (this ICoreContainer<IServerResolvable> container)
        {
            return container
                .Many<IRepositoryContext, RepositoryContext>();
        }
    }
}
