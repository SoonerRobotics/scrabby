using System.Collections.Generic;
using System.Linq;
using Scrabby.Networking;
using Scrabby.ScriptableObjects;
using Scrabby.Utilities;
using UnityEngine;

namespace Scrabby.State
{
    public class ScrabbyState : MonoSingleton<ScrabbyState>
    {
        public static bool ShowIncomingMessages = true;
        public static bool ShowOutgoingMessages = true;

        public NetworkType enabledNetworks = NetworkType.All;
        
        public List<Robot> robots;
        public List<Map> maps;
        public bool movementEnabled = true;
        public bool canMoveManually = false;
        public bool resetSceneOnConnectionLost = true;

        protected override void Init()
        {
            DontDestroyOnLoad(gameObject);
        }

        public Robot GetRobotById(string id)
        {
            if (robots.Any(x => x.id == id)) return robots.First(x => x.id == id);

            Debug.LogWarning("Robot with id " + id + " not found.");
            return null;
        }

        public Map GetMapById(string id)
        {
            if (maps.Any(x => x.id == id)) return maps.First(x => x.id == id);

            Debug.LogWarning("Map with id " + id + " not found.");
            return null;
        }

        public bool IsNetworkEnabled(NetworkType type)
        {
            return enabledNetworks.HasFlag(type);
        }
        
        public void SetNetworkEnabled(NetworkType type, bool isEnabled)
        {
            if (isEnabled)
            {
                enabledNetworks |= type;
            }
            else
            {
                enabledNetworks &= ~type;
            }
        }
    }
}