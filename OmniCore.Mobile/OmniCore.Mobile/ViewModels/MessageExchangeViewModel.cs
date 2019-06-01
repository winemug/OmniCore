using OmniCore.Model.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;

namespace OmniCore.Mobile.ViewModels
{
    public class MessageExchangeViewModel : BaseViewModel
    {
        private IMessageExchangeProgress ExchangeProgress;
        public MessageExchangeViewModel(IMessageExchangeProgress progress):base()
        {
            ExchangeProgress.PropertyChanged += ExchangeProgress_PropertyChanged;
        }

        private void ExchangeProgress_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (ExchangeProgress.Finished)
            {
                ExchangeProgress.PropertyChanged -= ExchangeProgress_PropertyChanged;
            }
        }
    }
}
