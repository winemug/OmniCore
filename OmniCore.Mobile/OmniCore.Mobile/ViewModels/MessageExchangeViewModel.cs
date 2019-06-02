using OmniCore.Model.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;

namespace OmniCore.Mobile.ViewModels
{
    public class MessageExchangeViewModel : BaseViewModel
    {
        private IMessageExchangeProgress exchangeProgress;
        public IMessageExchangeProgress ExchangeProgress { get => exchangeProgress; set => SetProperty(ref exchangeProgress, value); }
    }
}
