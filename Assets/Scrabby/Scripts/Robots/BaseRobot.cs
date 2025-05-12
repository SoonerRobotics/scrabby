using System.Collections.Generic;
using System.Linq;
using Scrabby.Interface;
using Scrabby.ScriptableObjects;
using Scrabby.State;
using UnityEngine;

namespace Scrabby.Robots
{
    public class BaseRobot : MonoBehaviour
    {
        private Vector3 _lastPosition;
        private float _lastSpeed;
        
        protected static void InitializeRobot(List<RobotOption> options)
        {
            // foreach (var topic in from option in options where option.key.StartsWith("topics.") && !option.key.EndsWith(".type") select option)
            // {
            //     SetupTopic(options, topic);
            // }
        }

        private static void SetupTopic(List<RobotOption> options, RobotOption option)
        {
            // var bits = option.key.Split(".");
            // if (bits.Length > 2)
            // {
            //     return;
            // }
            
            // var type = options.FirstOrDefault(x => x.key == $"{option.key}.type")?.value ?? "std_msgs/Empty";
            // RosConnector.Instance.Subscribe(option.value, type);
        }

        private void FixedUpdate()
        {
            var rotation = transform.rotation;
            HUDController.UpdateDegrees(rotation.eulerAngles.y);
            HUDController.UpdateRadians(rotation.eulerAngles.y * Mathf.Deg2Rad);
            
            var position = transform.position;
            HUDController.UpdateX(position.x);
            HUDController.UpdateY(position.z);
            
            var instSpeed = (position - _lastPosition).magnitude / Time.fixedDeltaTime;
            _lastSpeed = 0.85f * instSpeed + 0.15f * _lastSpeed;
            HUDController.UpdateMilesPerHour(_lastSpeed * 2.23693629f);
            HUDController.UpdateMetersPerSecond(_lastSpeed);
            _lastPosition = position;
            
            var length = Map.Active.originLength;
            var origin = Map.Active.origin;
            var latitude = position.z / length.x + origin.x;
            var longitude = position.x / length.y + origin.y;
            HUDController.UpdateLatitude(latitude);
            HUDController.UpdateLongitude(longitude);
            
            RobotUpdate();
        }

        protected virtual void RobotUpdate()
        {
            
        }

        protected bool CanMove()
        {
            return ScrabbyState.Instance.movementEnabled;
        }
    }
}