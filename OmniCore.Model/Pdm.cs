using OmniCore.Model.Enums;
using OmniCore.Model.Exceptions;
using OmniCore.Model.Interfaces;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Threading.Tasks;

namespace OmniCore.Model
{
    public class Pdm
    {
        private IPacketRadio packetRadio;

        private Pod _pod;
        public Pod Pod
        {
            get => this._pod;
            set
            {
                if (this._pod != value)
                {
                    this._pod = value;
                    if (value == null)
                    {
                        this.Nonce = null;
                    }
                    else
                    {
                        this.Nonce = new Nonce(value);
                    }
                }
            }
        }

        private Nonce Nonce { get; set; }

        public Pdm(IPacketRadio packetRadio)
        {
            this.packetRadio = packetRadio;
        }

    }
}
