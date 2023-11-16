using System.Collections.Generic;
using Newtonsoft.Json.Linq;
using Scrabby.Networking.PyScrabby;
using Scrabby.Networking.ROS;
using Scrabby.Networking.STORM;
using Scrabby.State;
using Scrabby.Utilities;

namespace Scrabby.Networking
{
    public class Network : MonoSingleton<Network>
    {
        public delegate void NetworkInstructionEvent(NetworkInstruction instruction);
        public static event NetworkInstructionEvent OnNetworkInstruction;
        public List<INetwork> Networks = new();

        private void Start()
        {
            Networks = new List<INetwork>();
            if (ScrabbyState.Instance.IsNetworkEnabled(NetworkType.Ros))
            {
                Networks.Add(new RosConnection());
            }

            if (ScrabbyState.Instance.IsNetworkEnabled(NetworkType.Storm))
            {
                Networks.Add(new StormConnection());
            }

            if (ScrabbyState.Instance.IsNetworkEnabled(NetworkType.PyScrabby))
            {
                Networks.Add(new PyScrabbyConnection());
            }
        }

        public void Initialize() 
        {
            Networks.ForEach(n => n.Init());
        }
        
        public PyScrabbyConnection GetPyScrabbyConnection()
        {
            return (PyScrabbyConnection) Networks.Find(n => n is PyScrabbyConnection);
        }

        public static void PublishNetworkInstruction(NetworkInstruction instruction)
        {
            OnNetworkInstruction?.Invoke(instruction);
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
            Networks.ForEach(n => n.Publish(topic, type, data));
        }

        public void Subscribe(string topic, string type)
        {
            Networks.ForEach(n => n.Subscribe(topic, type));
        }

        public void OnNetworkDisabled(NetworkType type)
        {
            if (type == NetworkType.Storm)
            {
                var stormConnection = (StormConnection) Networks.Find(n => n is StormConnection);
                stormConnection?.Close();
                Networks.RemoveAll(n => n is StormConnection);
            }
            else if (type == NetworkType.Ros)
            {
                var rosConnection = (RosConnection) Networks.Find(n => n is RosConnection);
                rosConnection?.Close();
                Networks.RemoveAll(n => n is RosConnection);
            }
            else if (type == NetworkType.PyScrabby)
            {
                var pyScrabbyConnection = (PyScrabbyConnection) Networks.Find(n => n is PyScrabbyConnection);
                pyScrabbyConnection?.Close();
                Networks.RemoveAll(n => n is PyScrabbyConnection);
            }
        }

        public void OnNetworkEnabled(NetworkType type)
        {
            if (type == NetworkType.Storm)
            {
                var n = new StormConnection();
                n.Init();
                Networks.Add(n);
            }
            else if (type == NetworkType.Ros)
            {
                var n = new RosConnection();
                n.Init();
                Networks.Add(n);
            }
            else if (type == NetworkType.PyScrabby)
            {
                var n = new PyScrabbyConnection();
                n.Init();
                Networks.Add(n);
            }
        }

        private void Update()
        {
            foreach (var network in Networks)
            {
                network.Update();
            }
        }

        public void Close()
        {
            Networks.ForEach(n => n.Close()); 
        }

        private void OnDestroy()
        {
            Networks.ForEach(n => n.Destroy()); 
        }

        private void OnApplicationQuit()
        {
            Networks.ForEach(n => n.Destroy()); 
        }
    }
}