using System;
using System.Collections.Generic;
using UnityEngine;
using NativeWebSocket;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Scrabby.Networking.ROS
{
    public class RosConnection : INetwork
    {
        private WebSocket _socket;
        private bool _dead;

        private readonly List<string> _subscriptions = new();
        private readonly List<string> _advertisements = new();
        private int _sequence;
        private bool _initialized = false;
        
        private Queue<string> _messageQueue = new();

        public void Init()
        {
            if (_initialized)
            {
                Debug.LogWarning("[ROS] Already initialized");
                return;
            }

            Debug.Log("[ROS] Initializing");
            _dead = false;
            
            _socket = new WebSocket("ws://localhost:9090");
            _socket.OnClose += OnClose;
            _socket.OnError += OnError;
            _socket.OnMessage += OnMessage;
            _socket.OnOpen += OnOpen;

            Connect();
        }

        private bool SendJson(JObject jObject)
        {
            if (!_initialized)
            {
                _messageQueue.Enqueue(JsonConvert.SerializeObject(jObject));
                return true;
            }
            
            var json = JsonConvert.SerializeObject(jObject);
            if (_socket is not { State: WebSocketState.Open })
            {
                Debug.LogWarning("[ROS] Socket is not open");
                return false;
            }

            _socket.SendText(json);
            return true;
        }

        private async void Connect()
        {
            switch (_dead)
            {
                case false:
                    await System.Threading.Tasks.Task.Delay(TimeSpan.FromSeconds(1f));
                    break;
                case true:
                    return;
            }

            await _socket.Connect();
        }
        
        private void OnOpen()
        {
            _initialized = true;
            while (_messageQueue.Count > 0)
            {
                var message = _messageQueue.Dequeue();
                _socket.SendText(message);
            }
        }

        private void OnClose(WebSocketCloseCode code)
        {
            _initialized = false;
            Connect();
        }

        private void OnMessage(byte[] bytes)
        {
            var message = System.Text.Encoding.UTF8.GetString(bytes);
            var json = JObject.Parse(message);
            var op = json[RosField.Op]?.ToObject<string>();
            Debug.Log(message);

            switch (op)
            {
                case RosOpcode.Publish:
                {
                    var topic = json[RosField.Topic]?.ToObject<string>();
                    var data = json[RosField.Message]?.ToObject<JObject>();
                    Network.PublishNetworkInstruction(new NetworkInstruction(topic, data));
                }
                    break;

                default:
                    Debug.LogWarning($"[ROS] Unknown Opcode: {op}");
                    break;
            }
        }

        private static void OnError(string error)
        {
            if (error == "Unable to connect to the remote server")
            {
                return;
            }
            
            Debug.LogError($"[ROS] Error: {error}");
        }

        private void Advertise(string topic, string type)
        {
            if (_advertisements.Contains(topic))
            {
                return;
            }

            var id = $"advertise:{type}:{++_sequence}";
            var json = new JObject
            {
                { RosField.Op, RosOpcode.Advertise },
                { RosField.ID, id },
                { RosField.Topic, topic },
                { RosField.Type, type }
            };
            
            if (SendJson(json))
            {
                _advertisements.Add(topic);
            }
        }

        public void Subscribe(string topic, string type)
        {
            Debug.Log($"[ROS] Subscribing to {topic}");
            if (_subscriptions.Contains(topic))
            {
                Debug.Log($"[ROS] Already subscribed to {topic}");
                return;
            }

            var id = $"subscribe:{type}:{++_sequence}";
            Debug.Log($"[ROS] Subscribing to {topic} with id {id}");
            var json = new JObject
            {
                { RosField.Op, RosOpcode.Subscribe },
                { RosField.ID, id },
                { RosField.Topic, topic },
                { RosField.Type, type },
                { RosField.Compression, RosCompression.None },
                { RosField.ThrottleRate, 0 }
            };
            Debug.Log(json);

            if (SendJson(json))
            {
                Debug.Log($"[ROS] Subscribed to {topic}");
                _subscriptions.Add(topic);
            }
            else
            {
                Debug.LogWarning($"[ROS] Failed to subscribe to {topic}");
            }
        }

        public void Publish(string topic, string type, JObject data)
        {
            if (!_subscriptions.Contains(topic))
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

        public void Update()
        {
#if !UNITY_WEBGL || UNITY_EDITOR
            _socket?.DispatchMessageQueue();
#endif
        }

        public void Destroy()
        {
            _dead = true;
            _socket?.Close();
        }

        public void Close()
        {
            Destroy();
        }
    }

    internal struct TopicInfo
    {
        public readonly string ID;
        public readonly string Type;

        public TopicInfo(string id, string type)
        {
            ID = id;
            Type = type;
        }
    }
}