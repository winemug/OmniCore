using OmniCore.Model.Interfaces.Entities;
using OmniCore.Model.Interfaces.Operational;
using System;
using System.Collections.Generic;
using System.Text;

namespace OmniCore.Model.Interfaces.Services
{
    public interface IPodService
    {
        IPodProvider ErosProvider { get; }
        IPodProvider DashProvider { get; }
        IPodProvider GetRemoteProvider(IUserEntity user);
    }
}
