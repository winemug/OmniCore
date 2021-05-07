using System.Collections.ObjectModel;

namespace OmniCore.Model.Interfaces
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