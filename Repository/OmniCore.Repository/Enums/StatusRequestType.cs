using System;
using System.Collections.Generic;
using System.Text;

namespace OmniCore.Repository.Enums
{
    public enum StatusRequestType
    {
        Standard = 0x00,
        ExpiredAlertStatus = 0x01,
        FaultEventInformation = 0x02,
        DataLogContents = 0x03,
        FaultInformationWithPodInitializationTime = 0x05,
        HardcodedValues = 0x06,
        FlashVariables = 0x46,
        DumpEntries = 0x50,
        DumpPreviousEntries = 0x51
    }
}
