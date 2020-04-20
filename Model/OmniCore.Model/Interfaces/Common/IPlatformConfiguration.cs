using System;
using System.Threading.Tasks;
using OmniCore.Model.Interfaces.Client;
using OmniCore.Model.Interfaces.Services;

namespace OmniCore.Model.Interfaces.Common
{
    public interface IPlatformConfiguration 
    {
        bool ServiceEnabled { get; set; }
        bool TermsAccepted { get; set; }
        bool DefaultUserSetUp { get; set; }
    }
}