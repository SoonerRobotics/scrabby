using Newtonsoft.Json.Linq;

namespace Scrabby.Networking
{
    public interface INetwork
    {
        public void Init();
        public void Publish(string topic, string type, JObject data);
        public void Update();
        public void Subscribe(string topic, string type);
        public void Destroy();
        public void Close();
    }
}