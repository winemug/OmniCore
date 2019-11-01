using System;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using OmniCore.Model.Enums;
using OmniCore.Model.Interfaces;
using SQLite;

namespace OmniCore.Eros
{
    public class ErosResult : IPodResult<ErosPod>
    {
        public Guid Id { get; set; }
        public DateTimeOffset Created { get; set; }
        public DateTimeOffset Updated { get; set; }

        [JsonConverter(typeof(StringEnumConverter))]
        public ResultType ResultType { get; set; }

        [Ignore]
        public Exception Exception { get; set; }

    }
}
