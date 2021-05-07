namespace OmniCore.Model.Interfaces
{
    public interface IPlatformConfiguration 
    {
        bool ServiceEnabled { get; set; }
        bool TermsAccepted { get; set; }
        bool DefaultUserSetUp { get; set; }
    }
}