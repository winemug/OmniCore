using OmniCore.Model.Interfaces.Data;
using System;
using System.Collections.Generic;
using System.Text;

namespace OmniCore.Mobile.ViewModels.Pod
{
    class ResultViewModel : BaseViewModel
    {
        private IMessageExchangeResult messageExchangeResult;
        public IMessageExchangeResult MessageExchangeResult
        {
            get => messageExchangeResult;
            set
            {
                if (messageExchangeResult != value)
                {
                    if (messageExchangeResult != null)
                        messageExchangeResult.PropertyChanged -= MessageExchangeResult_PropertyChanged;

                    messageExchangeResult = value;

                    if (messageExchangeResult != null)
                        messageExchangeResult.PropertyChanged += MessageExchangeResult_PropertyChanged;

                    OnPropertyChanged(nameof(MessageExchangeResult));
                }
            }
        }

        private void MessageExchangeResult_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
        }

        protected override void OnDisposeManagedResources()
        {
            MessageExchangeResult = null;
        }
    }
}
