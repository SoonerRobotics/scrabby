using Newtonsoft.Json.Linq;

namespace Scrabby.Networking
{
    public class NetworkInstruction
    {
        public string Topic;
        private readonly JObject _data;

        public NetworkInstruction(string topic, string data)
        {
            Topic = topic;
            _data = JObject.Parse(data);
        }

        public NetworkInstruction(string topic, JObject data)
        {
            Topic = topic;
            _data = data;
        }
        
        public T GetData<T>(string key)
        {
            return !_data.ContainsKey(key) ? default : _data[key]!.ToObject<T>();
        }
        
        public T GetData<T>(string key, T defaultValue)
        {
            return !_data.ContainsKey(key) ? defaultValue : _data[key]!.ToObject<T>();
        }

        public override string ToString()
        {
            return $"[NetworkInstruction] Topic: {Topic}, Data: {_data}";
        }
    }
}