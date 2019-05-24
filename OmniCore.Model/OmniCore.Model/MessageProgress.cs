using OmniCore.Model.Interfaces;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;

namespace OmniCore.Model
{
    public class MessageProgress : IMessageProgress
    {
        public bool CanBeCanceled => throw new NotImplementedException();

        public bool Queued => throw new NotImplementedException();

        public bool Running => throw new NotImplementedException();

        public int Progress => throw new NotImplementedException();

        public bool Finished => throw new NotImplementedException();

        public bool Successful => throw new NotImplementedException();

        public event PropertyChangedEventHandler PropertyChanged;
    }
}
