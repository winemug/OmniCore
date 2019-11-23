using OmniCore.Model.Interfaces.Entities;
using System;
using System.Collections.Generic;
using System.Text;
using OmniCore.Model.Interfaces.Workflow;

namespace OmniCore.Model.Interfaces.Services
{
    public interface IPodService
    {
        IPodProvider ErosProvider { get; }
        IPodProvider DashProvider { get; }
        IPodProvider GetRemoteProvider(IUserEntity user);
    }
}
