using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;
using OmniCore.Model.Interfaces.Platform.Common;

namespace OmniCore.Model.Utilities
{
    public class TaskProgress : ITaskProgress
    {
        public double? PercentageComplete { get; set; }
        public ObservableCollection<(ISubTaskProgress SubTask, double? TaskWeight)> SubTasks { get; set; }

        public TaskProgress()
        {
            PercentageComplete = null;
        }

        public ISubTaskProgress AddSubTask(string subTaskName, string subTaskDescription = null, double? weight = null)
        {
            var st = new SubTaskProgress
            {
                Name = subTaskName,
                Description = subTaskDescription
            };
            SubTasks.Add((st, weight));
            return st;
        }
    }
}
