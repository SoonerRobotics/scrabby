using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using Newtonsoft.Json.Linq;
using Scrabby.State;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Scrabby.Networking.PyScrabby
{
    public class PyScrabbyConnection : INetwork
    {
        public readonly int Port = 9092;

        private TcpListener _listener;
        private NetworkStream _clientStream;
        private TcpClient _client;
        private readonly Queue<string> _incomingMessages = new();
        private Thread _thread;
        private bool _needsReset = false;

        public void Init()
        {
            _listener = new TcpListener(IPAddress.Loopback, Port);
            _listener.Start();

            _thread = new Thread(TcpWorker);
            _thread.Start();
        }

        private void TcpWorker()
        {
            while (true)
            {
                Debug.Log("[PyScrabby] Waiting for client...");
                _client = _listener.AcceptTcpClient();
                _clientStream = _client.GetStream();
                var buffer = new byte[_client.ReceiveBufferSize];

                Debug.Log("[PyScrabby] Client connected");
                while (_client.Connected)
                {
                    int bytesRead;
                    try
                    {
                        bytesRead = _clientStream.Read(buffer, 0, _client.ReceiveBufferSize);
                    }
                    catch (Exception)
                    {
                        break;
                    }

                    var message = System.Text.Encoding.ASCII.GetString(buffer, 0, bytesRead);
                    Debug.Log("[PyScrabby] Received: " + message);
                    _incomingMessages.Enqueue(message);
                }

                _clientStream = null;
                Debug.Log("[PyScrabby] Client disconnected");

                if (ScrabbyState.instance.resetSceneOnConnectionLost)
                {
                    _needsReset = true;
                }
            }

// ReSharper disable once FunctionNeverReturns
        }

        public void Publish(string topic, string type, JObject data)
        {
            JObject message = new()
            {
                ["op"] = "publish",
                ["topic"] = topic,
                ["type"] = type,
                ["data"] = data
            };

            string json;
            try
            {
                json = message.ToString();
            }
            catch (Exception e)
            {
                Debug.LogError("[PyScrabby] Failed to serialize message: " + e.Message);
                return;
            }

            if (_clientStream == null)
            {
                return;
            }

            try
            {
                var bytes = System.Text.Encoding.ASCII.GetBytes(json);
                _clientStream.Write(bytes, 0, bytes.Length);
                _clientStream.Flush();
            }
            catch (Exception e)
            {
                Debug.LogError("[PyScrabby] Failed to send message: " + e.Message);
            }
        }

        public void Update()
        {
            if (_needsReset)
            {
                _needsReset = false;
                ScrabbyState.instance.movementEnabled = true;
                SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
                return;
            }
            
            if (_incomingMessages.Count == 0)
            {
                return;
            }

            var message = _incomingMessages.Dequeue();
            JObject json;
            try
            {
                json = JObject.Parse(message);
            }
            catch (Exception e)
            {
                Debug.LogError("[PyScrabby] Failed to parse message: " + e.Message);
                return;
            }

            var op = json["op"]?.ToString();
            var topic = json["topic"]?.ToString();
            if (op == null || topic == null || !json.TryGetValue("msg", out var data))
            {
                Debug.LogError("[PyScrabby] Invalid message received");
                return;
            }

            Debug.Log("[PyScrabby] Received message: " + message);
            Network.PublishNetworkInstruction(new NetworkInstruction(topic, data.ToObject<JObject>()));
        }

        public void Subscribe(string topic, string type)
        {
        }

        public void Close()
        {
            _client?.Close();
            _clientStream?.Close();
        }

        public void Destroy()
        {
            _client?.Close();
            _clientStream?.Close();
            _thread.Abort();
            _listener.Stop();
        }
    }
}