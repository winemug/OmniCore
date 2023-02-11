using System.Text;

namespace OmniCore.Services.Interfaces
{
    public class AmqpMessage
    {
        public byte[] Body { get; set; }
        
        public string Id { get; set; }
        
        public string Text
        {
            get
            {
                return Encoding.ASCII.GetString(Body); 
            }
            set
            {
                Body = Encoding.ASCII.GetBytes(value);
            }
        }
    }
}