﻿using System;
using Xamarin.Forms;

namespace OmniCore.Client.ViewModels.Base
{
    public class SubscriberCommand : Command
    {
        public SubscriberCommand(Action<object> execute) : base(execute)
        {
        }

        public SubscriberCommand(Action execute) : base(execute)
        {
        }

        public SubscriberCommand(Action<object> execute, Func<object, bool> canExecute) : base(execute, canExecute)
        {
        }

        public SubscriberCommand(Action execute, Func<bool> canExecute) : base(execute, canExecute)
        {
        }
    }
}