using System.Collections.ObjectModel;

namespace OmniCore.Model.Interfaces.Common
{
    public interface ITaskProgress
    {
        double? PercentageComplete { get; set; }
        ObservableCollection<(ISubTaskProgress SubTask, double? TaskWeight)> SubTasks { get; set; }
        ISubTaskProgress AddSubProgress(string subTaskName, string subTaskDescription = null, double? weight = null);
    }
}
