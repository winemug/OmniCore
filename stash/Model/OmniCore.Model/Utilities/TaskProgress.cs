using System.Collections.ObjectModel;
using OmniCore.Model.Interfaces;

namespace OmniCore.Model.Utilities
{
    public class TaskProgress : ITaskProgress
    {
        public TaskProgress()
        {
            Children = new ObservableCollection<ITaskProgress>();
        }

        public string Name { get; set; }
        public string Description { get; set; }
        public double? PercentComplete { get; set; }
        public double? Weight { get; set; }
        public bool InProgress { get; set; }

        public ObservableCollection<ITaskProgress> Children { get; set; }
    }
}