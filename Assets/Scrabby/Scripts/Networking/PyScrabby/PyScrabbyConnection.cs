using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Runtime.CompilerServices;
using System.Threading;
using Newtonsoft.Json.Linq;
using Scrabby.State;
using UnityEngine;

namespace Scrabby.Networking.PyScrabby
{
    public class PyScrabbyConnection : INetwork
    {
        private TcpListener _tcp;
        private readonly List<Client> _clients = new();
        private readonly object _clientLock = new();
        private readonly Queue<string> _messageQueue = new();

        private Thread _serverThread;
        private Thread _readThread;

        private bool _isRunning = true;
        private const int Port = 9092;

        public void Init()
        {
            _tcp = new TcpListener(IPAddress.Loopback, Port);
            _tcp.Start();
            
            _serverThread = new Thread(ServerThread);
            _serverThread.Start();

            _readThread = new Thread(ReadThread);
            _readThread.Start();
        }

        public void Publish(string topic, string type, JObject data)
        {
            JObject message = new()
            {
                ["op"] = "publish",
                ["topic"] = topic,
                ["msg"] = data
            };
            var json = message.ToString();
            if (ScrabbyState.ShowOutgoingMessages)
            {
                Debug.Log($"[PyScrabby] Sending: {json}");
            }

            lock (_clientLock)
            {
                for (var i = _clients.Count - 1; i >= 0; i--)
                {
                    var client = _clients[i];
                    try
                    {
                        client.Send(json);
                    }
                    catch (Exception)
                    {
                        _clients.RemoveAt(i);
                    }
                }
            }
        }

        public void Destroy()
        {
            _isRunning = false;
            _serverThread?.Abort();
            _readThread?.Abort();
            _tcp?.Stop();
            lock (_clientLock)
            {
                foreach (var client in _clients)
                {
                    client.Close();
                }
                
                _clients.Clear();
            }
            
            _serverThread = null;
            _readThread = null;
            _tcp = null;
        }
        
        public int GetPort()
        {
            return Port;
        }
        
        public int GetNumClients()
        {
            lock (_clientLock)
            {
                return _clients.Count;
            }
        }
        
        public int MessagesInQueue()
        {
            return _messageQueue.Count;
        }

        private void ServerThread()
        {
            while (_isRunning)
            {
                var client = _tcp.AcceptTcpClient();
                lock (_clientLock)
                {
                    _clients.Add(new Client(client));
                }
            }
        }

        private void ReadThread()
        {
            while (_isRunning)
            {
                lock (_clientLock)
                {
                    for (var index = 0; index < _clients.Count; index++)
                    {
                        var client = _clients[index];
                        if (!client.HasData())
                        {
                            continue;
                        }

                        var message = client.Receive();
                        if (ScrabbyState.ShowIncomingMessages)
                        {
                            Debug.Log($"[PyScrabby] Received: {message}");
                        }
                        _messageQueue.Enqueue(message);
                    }
                }
            }
        }

        private void RemoveDeadClients()
        {
            var removedClients = new List<Client>();
            lock (_clientLock)
            {
                removedClients.AddRange(_clients.Where(client => client.IsDead()));

                foreach (var client in removedClients)
                {
                    _clients.Remove(client);
                }
            }
        }

        public void Update()
        {
            if (_messageQueue.Count == 0)
            {
                return;
            }

            var message = _messageQueue.Dequeue();
            JObject json;
            try
            {
                json = JObject.Parse(message);
            } catch (Exception)
            {
                Debug.LogError($"Failed to parse message: {message}");
                return;
            }
            var op = json["op"]?.ToString();
            var topic = json["topic"]?.ToString();
            if (op == null || topic == null || !json.TryGetValue("msg", out var data))
            {
                return;
            }

            Network.PublishNetworkInstruction(new NetworkInstruction(topic, data.ToObject<JObject>()));
        }

        public void Subscribe(string topic, string type)
        {
        }
    }
    
    internal readonly struct Client
    {
        private readonly TcpClient _client;
        private readonly NetworkStream _stream;

        public Client(TcpClient client)
        {
            _client = client;
            _stream = client.GetStream();
        }

        public void Send(string message)
        {
            var bytes = System.Text.Encoding.UTF8.GetBytes(message);
            _stream.Write(bytes, 0, bytes.Length);
        }

        public string Receive()
        {
            var bytes = new byte[_client.ReceiveBufferSize];
            var read = _stream.Read(bytes, 0, _client.ReceiveBufferSize);
            return System.Text.Encoding.UTF8.GetString(bytes, 0, read);
        }

        public bool HasData()
        {
            return _stream.DataAvailable;
        }

        public bool IsDead()
        {
            return !_client.Connected;
        }

        public void Close()
        {
            _stream.Close();
            _client.Close();
        }
    }
}