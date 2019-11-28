using OmniCore.Model.Interfaces.Entities;
using System;
using System.Collections.Generic;
using System.Text;
using OmniCore.Model.Interfaces.Workflow;

namespace OmniCore.Model.Interfaces.Services
{
    public interface IPodService
    {
        IPodProvider[] Providers{ get; }
        IPodProvider GetRemoteProvider(IUserEntity user);
    }
}
