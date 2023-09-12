using System.Collections.Generic;
using System.Linq;
using Scrabby.ScriptableObjects;
using Scrabby.Utilities;
using UnityEngine;

namespace Scrabby.State
{
    public class ScrabbyState : MonoSingleton<ScrabbyState>
    {
        public static bool ShowIncomingMessages = true;
        public static bool ShowOutgoingMessages = true;
        
        public List<Robot> robots;
        public List<Map> maps;
        public bool movementEnabled = true;
        public bool canMoveManually = false;
        public bool resetSceneOnConnectionLost = true;

        protected override void Init()
        {
            DontDestroyOnLoad(gameObject);
            
            Screen.SetResolution(Display.main.systemHeight, Display.main.systemWidth, false);
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
    }
}