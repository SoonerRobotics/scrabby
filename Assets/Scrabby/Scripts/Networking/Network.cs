using System.Collections.Generic;
using Newtonsoft.Json.Linq;
using Scrabby.Networking.PyScrabby;
using Scrabby.Networking.ROS;
using Scrabby.Networking.STORM;
using Scrabby.Utilities;

namespace Scrabby.Networking
{
    public class Network : MonoSingleton<Network>
    {
        public delegate void NetworkInstructionEvent(NetworkInstruction instruction);
        public static event NetworkInstructionEvent onNetworkInstruction;

        private List<INetwork> _networks = new();

        private void Start()
        {
            _networks = new List<INetwork>
            {
                new PyScrabbyConnection(),
                new RosConnection(),
                new StormConnection()
            };
            
            _networks.ForEach(n => n.Init());
        }
        
        public PyScrabbyConnection GetPyScrabbyConnection()
        {
            return (PyScrabbyConnection) _networks.Find(n => n is PyScrabbyConnection);
        }

        public static void PublishNetworkInstruction(NetworkInstruction instruction)
        {
            onNetworkInstruction?.Invoke(instruction);
        }

        public void PublishCompressedImage(string topic, byte[] data)
        {
            var obj = new JObject
            {
                { RosField.Format, "jpeg" },
                { RosField.Header, new JObject() },
                { RosField.Data, System.Convert.ToBase64String(data) }
            };

            Publish(topic, "sensor_msgs/CompressedImage", obj);
        }

        public void Publish(string topic, string type, JObject data)
        {
            _networks.ForEach(n => n.Publish(topic, type, data));
        }

        public void Subscribe(string topic, string type)
        {
            _networks.ForEach(n => n.Subscribe(topic, type));
        }

        private void Update()
        {
            foreach (var network in _networks)
            {
                network.Update();
            }
        }

        private void OnDestroy()
        {
            _networks.ForEach(n => n.Destroy()); 
        }

        private void OnApplicationQuit()
        {
            _networks.ForEach(n => n.Destroy());
        }
    }
}