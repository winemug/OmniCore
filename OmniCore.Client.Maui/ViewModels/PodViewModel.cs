using OmniCore.Common.Core;

namespace OmniCore.Maui.ViewModels;

public class PodViewModel
{
    private IPodService _podService;
    
    public PodViewModel(IPodService podService)
    {
        _podService = podService;
    }
}
