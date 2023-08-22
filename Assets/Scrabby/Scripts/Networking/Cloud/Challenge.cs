using System;
using Newtonsoft.Json;
using UnityEngine.Serialization;

namespace Scrabby.Networking
{
    [Serializable]
    public class Challenge
    {
        public string id;
        public string name;
        public string description;
        public string type;
        [JsonProperty("robot_id")]
        public string robotId;
        [JsonProperty("map_id")]
        public string mapId;
        public uint timeout;
    }
}