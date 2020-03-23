using System.Collections.ObjectModel;
using OmniCore.Model.Interfaces.Services.Facade;

namespace OmniCore.Model.Interfaces.Common
{
    public interface ITaskProgress
    {
        string Name { get; set; }
        string Description { get; set; }
        double? PercentComplete { get; set; }
        double? Weight { get; set; }
        bool InProgress { get; set; }
        ObservableCollection<ITaskProgress> Children { get; set; }
    }
}
