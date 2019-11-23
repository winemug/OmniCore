using OmniCore.Model.Interfaces.Entities;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using OmniCore.Model.Interfaces.Workflow;

namespace OmniCore.Radios.RileyLink
{
    public class RileyLinkRadio : IRadio
    {
        public IRadioEntity Entity { get; set; }

        public Task<IRadioConnection> GetConnection()
        {
            throw new NotImplementedException();
        }

        public Task<string> GetDefaultConfiguration()
        {
            throw new NotImplementedException();
        }
    }
}
