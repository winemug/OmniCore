using System;
using OmniCore.Model.Interfaces.Services;

namespace OmniCore.Client.Droid.Services
{
    public class ApplicationService : IApplicationService
    {
        public Version ApplicationVersion { get; }
    }
}