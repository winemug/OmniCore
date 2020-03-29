using System;
using System.Collections.Generic;
using System.Text;
using OmniCore.Model.Interfaces.Client;
using OmniCore.Model.Interfaces.Services.Facade;

namespace OmniCore.Client.ViewModels.Base
{
    public class TaskViewModel : BaseViewModel
    {
        public ITask CurrentTask { get; private set; }
        public TaskViewModel(ICoreClient client) : base(client)
        {
        }

        protected void SetTask(ITask task)
        {
            CurrentTask = task;
            task.WhenCanCancelChanged()
                .Subscribe(canCancel =>
                {
                    
                });

            task.WhenStateChanged()
                .Subscribe(state =>
                {
                    
                });

            task.WhenResultReceived()
                .Subscribe(result =>
                {
                    
                });
        }
    }
}
