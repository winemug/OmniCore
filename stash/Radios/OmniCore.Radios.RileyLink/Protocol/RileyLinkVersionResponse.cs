﻿using System.Text;

namespace OmniCore.Radios.RileyLink.Protocol
{
    public class RileyLinkVersionResponse : RileyLinkStandardResponse
    {
        public string VersionString { get; private set; }

        protected override void ParseInternal(byte[] responseData)
        {
            VersionString = Encoding.ASCII.GetString(responseData);
        }
    }
}