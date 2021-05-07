﻿using System;
using OmniCore.Model.Enumerations;

namespace OmniCore.Model.Exceptions
{
    public class OmniCorePeripheralException : OmniCoreException
    {
        public OmniCorePeripheralException(FailureType failureType, string message = null, Exception inner = null) :
            base(failureType, message, inner)
        {
        }
    }
}