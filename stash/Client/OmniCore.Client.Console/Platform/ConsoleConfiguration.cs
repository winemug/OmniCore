using OmniCore.Model.Interfaces;

namespace OmniCore.Client.Console.Platform
{
    public class ConsoleConfiguration : IPlatformConfiguration
    {
        public bool ServiceEnabled { get; set; } = true;
        public bool TermsAccepted { get; set; } = true;
        public bool DefaultUserSetUp { get; set; } = true;
    }
}