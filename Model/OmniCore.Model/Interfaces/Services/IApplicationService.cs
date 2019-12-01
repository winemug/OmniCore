using System;
using System.Collections.Generic;
using System.Runtime.Versioning;
using System.Text;

namespace OmniCore.Model.Interfaces.Services
{
    public interface IApplicationService
    {
        Version ApplicationVersion { get; }
    }
}
