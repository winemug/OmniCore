using System;
using OmniCore.Model.Interfaces;

namespace OmniCore.Client.ViewModels.Base
{
    public class TaskViewModel : BaseViewModel
    {
        public TaskViewModel(IClient client) : base(client)
        {
        }

        public ITask CurrentTask { get; private set; }

        protected void SetTask(ITask task)
        {
            CurrentTask = task;
            task.WhenCanCancelChanged()
                .Subscribe(canCancel => { });

            task.WhenStateChanged()
                .Subscribe(state => { });

            task.WhenResultReceived()
                .Subscribe(result => { });
        }
    }
}