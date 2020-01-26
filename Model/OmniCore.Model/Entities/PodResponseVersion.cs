namespace OmniCore.Model.Entities
{
    public class PodResponseVersion
    {
        public string VersionPm { get; set; }
        public string VersionPi { get; set; }
        public byte[] VersionUnk2b { get; set; }
        public uint Lot { get; set; }
        public uint Serial { get; set; }
        public uint RadioAddress { get; set; }
    }
}