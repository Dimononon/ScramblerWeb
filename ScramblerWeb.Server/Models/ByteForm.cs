using Services.Scramblers;

namespace ScramblerWeb.Server.Models
{
    public class ByteForm
    {
        public string Key { get; set; }
        public List<byte> Data { get; set; }
        public List<ScramblerType> Algorithms { get; set; }
    }
}
