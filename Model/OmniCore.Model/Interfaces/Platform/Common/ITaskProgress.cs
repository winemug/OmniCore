using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;

namespace OmniCore.Model.Interfaces.Platform.Common
{
    public interface ITaskProgress
    {
        double? PercentageComplete { get; set; }
        ObservableCollection<(ISubTaskProgress SubTask, double? TaskWeight)> SubTasks { get; set; }
        ISubTaskProgress AddSubTask(string subTaskName, string subTaskDescription = null, double? weight = null);
    }
}
