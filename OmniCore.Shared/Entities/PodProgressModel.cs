using OmniCore.Shared.Enums;

namespace OmniCore.Shared.Entities;

public class PodProgressModel
{
    public PodProgress Progress { get; set; }
    public bool Faulted { get; set; }
}