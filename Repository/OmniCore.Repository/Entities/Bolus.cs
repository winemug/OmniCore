using System;
using System.Collections.Generic;
using System.Text;

namespace OmniCore.Repository.Entities
{
    public class Bolus
    {
        public long RequestId { get; set; }
        public int Requested { get; set; }
        public int Delivered { get; set; }
        public DateTimeOffset Started { get; set; }
        public DateTimeOffset Finished { get; set; }
    }
}
