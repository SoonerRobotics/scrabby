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

        public void Init()
        {
            _socket = new WebSocket("ws://localhost:9090");
            _socket.OnClose += OnClose;
            _socket.OnError += OnError;
            _socket.OnMessage += OnMessage;

            Connect();
        }

        private bool SendJson(JObject jObject)
        {
            var json = JsonConvert.SerializeObject(jObject);
            if (_socket is not { State: WebSocketState.Open })
            {
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

        private void OnClose(WebSocketCloseCode code)
        {
            Connect();
        }

        private void OnMessage(byte[] bytes)
        {
            var message = System.Text.Encoding.UTF8.GetString(bytes);
            var json = JObject.Parse(message);
            var op = json[RosField.Op]?.ToObject<string>();

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
            if (_subscriptions.Contains(topic))
            {
                return;
            }

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

            if (SendJson(json))
            {
                _subscriptions.Add(topic);
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