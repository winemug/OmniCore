using OmniCore.Model.Enumerations;

namespace OmniCore.Model.Interfaces.Common
{
    public interface ISubTaskProgress
    {
        double? PercentComplete { get; set; }
        string Name { get; set; }
        string Description { get; set; }
        ProgressPhase Phase { get; set; }
        string ActivityDetail { get; set; }
    }
}
