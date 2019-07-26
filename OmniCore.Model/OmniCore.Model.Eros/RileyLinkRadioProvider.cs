using System;
using System.Threading.Tasks;
using OmniCore.Model.Interfaces;

namespace OmniCore.Model.Eros
{
    public class RileyLinkRadioProvider : IRadioProvider
    {
        public Task<IRadio> FirstAvailable()
        {
            throw new NotImplementedException();
        }
    }
}
