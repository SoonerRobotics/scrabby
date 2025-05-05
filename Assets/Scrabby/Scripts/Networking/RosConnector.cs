using System;
using System.Collections.Generic;
using NativeWebSocket;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Scrabby.Networking.ROS;
using Scrabby.Utilities;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;

namespace Scrabby.Networking
{
    public class RosConnector : MonoSingleton<RosConnector>
    {
        public static readonly UnityEvent<NetworkInstruction> OnNetworkInstruction = new();
        
        private WebSocket _socket;
        private bool _wasAlive;
        
        private readonly List<KeyValuePair<string, string>> _subscriptions = new();
        private readonly List<KeyValuePair<string, string>> _advertisements = new();
        private int _sequence;

        [Header("Connection Settings")]
        public int port;
        public string host;
        public bool secure;
        public float reconnectDelay = 1f;
        
        private void Start()
        {
            _socket = new WebSocket($"ws{(secure ? "s" : "")}://{host}:{port}");
            _socket.OnOpen += OnSocketOpened;
            _socket.OnClose += OnSocketClosed;
            _socket.OnMessage += OnSocketMessage;
            _socket.OnError += OnSocketErrored;
            
            Connect();
        }

        private async void Connect()
        {
            await _socket.Connect();
        }

        private void OnSocketOpened()
        {
            _wasAlive = true;
            Debug.Log("[RosConnector.OnSocketOpened] Connected");
            
            foreach (var advertisement in _advertisements)
            {
                Advertise(advertisement.Key, advertisement.Value);
            }
            
            foreach (var subscription in _subscriptions)
            {
                Subscribe(subscription.Key, subscription.Value);
            }
        }

        private void OnSocketClosed(WebSocketCloseCode code)
        {
            Debug.LogWarning($"[RosConnector.OnSocketClosed] {code}");

            if (_wasAlive)
            {
                // Reload the scene
                _wasAlive = false;
                SceneManager.LoadScene(SceneManager.GetActiveScene().name);
            }
            Invoke(nameof(Connect), reconnectDelay);
        }

        private void OnSocketMessage(byte[] data)
        {
            string message;
            try
            {
                message = System.Text.Encoding.UTF8.GetString(data);
                Debug.Log($"[RosConnector.OnSocketMessage] {message}");
            }
            catch (Exception e)
            {
                Debug.LogError($"[RosConnector.OnSocketMessage] {e.Message}");
                return;
            }
            
            var json = JObject.Parse(message);
            var op = json[RosField.Op]?.ToObject<string>();

            switch (op)
            {
                case RosOpcode.Publish:
                {
                    var topic = json[RosField.Topic]?.ToObject<string>();
                    var jData = json[RosField.Message]?.ToObject<JObject>();
                    Debug.Log($"[RosConnector.OnSocketMessage] {topic} {jData}");
                    OnNetworkInstruction.Invoke(new NetworkInstruction(topic, jData));
                }
                    break;

                default:
                    Debug.LogWarning($"[RosConnector.OnSocketMessage] Unknown Opcode: {op}");
                    break;
            }
        }
        
        private void SendJson(JObject json)
        {
            if (_socket.State != WebSocketState.Open)
            {
                return; 
            }

            string msg = JsonConvert.SerializeObject(json);
            _socket.SendText(msg);
            return;
        }
        
        public void Subscribe(string topic, string type)
        {

            var id = $"subscribe:{type}:{++_sequence}";
            var json = new JObject
            {
                { RosField.Op, RosOpcode.Subscribe },
                { RosField.ID, id },
                { RosField.Topic, topic },
                { RosField.Type, type },
                { RosField.Compression, RosCompression.None },
                { RosField.ThrottleRate, 0 }
            };

            if (!_subscriptions.Contains(new KeyValuePair<string, string>(topic, type)))
            {
                _subscriptions.Add(new KeyValuePair<string, string>(topic, type));
            }
            else
            {
                return;
            }

            Debug.Log($"[RosConnector.Subscribe] {json}");
            SendJson(json);
        }
        
        private void Advertise(string topic, string type)
        {
            var id = $"advertise:{type}:{++_sequence}";
            var json = new JObject
            {
                { RosField.Op, RosOpcode.Advertise },
                { RosField.ID, id },
                { RosField.Topic, topic },
                { RosField.Type, type }
            };
            
            if (!_advertisements.Contains(new KeyValuePair<string, string>(topic, type)))
            {
                _advertisements.Add(new KeyValuePair<string, string>(topic, type));
            }
            else
            {
                return;
            }
            
            Debug.Log($"[RosConnector.Advertise] {json}");
            SendJson(json);
        }
        
        public void Publish(string topic, string type, JObject data)
        {
            if (!_advertisements.Contains(new KeyValuePair<string, string>(topic, type)))
            {
                Advertise(topic, type);
            }

            var id = $"publish:{type}:{++_sequence}";
            var json = new JObject
            {
                { RosField.Op, RosOpcode.Publish },
                { RosField.Topic, topic },
                { RosField.ID, id },
                { RosField.Message, data }
            };
            SendJson(json);
        }

        private async void OnSocketErrored(string msg)
        {
            if (msg.Contains("Unable to connect to the remote server"))
            {
                await _socket.Close();
                return;
            }
            Debug.LogError($"[RosConnector.OnSocketErrored] {msg}");
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
        
        public void Update()
        {
#if !UNITY_WEBGL || UNITY_EDITOR
            _socket?.DispatchMessageQueue();
#endif
        }
    }
}