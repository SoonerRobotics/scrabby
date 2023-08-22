using System.Collections.Generic;
using System.Linq;
using Scrabby.ScriptableObjects;
using Scrabby.State;
using UnityEngine;
using Network = Scrabby.Networking.Network;

namespace Scrabby.Robots
{
    public class BaseRobot : MonoBehaviour
    {
        protected static void InitializeRobot(List<RobotOption> options)
        {
            foreach (var topic in from option in options where option.key.StartsWith("topics.") && !option.key.EndsWith(".type") select option)
            {
                SetupTopic(options, topic);
            }
        }

        private static void SetupTopic(List<RobotOption> options, RobotOption option)
        {
            var bits = option.key.Split(".");
            if (bits.Length > 2)
            {
                return;
            }
            
            var type = options.FirstOrDefault(x => x.key == $"{option.key}.type")?.value ?? "std_msgs/Empty";
            Network.instance.Subscribe(option.value, type);
        }

        protected bool CanMove()
        {
            return ScrabbyState.instance.movementEnabled;
        }
    }
}