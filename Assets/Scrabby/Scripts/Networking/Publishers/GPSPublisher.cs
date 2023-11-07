using System;
using Newtonsoft.Json.Linq;
using Scrabby.ScriptableObjects;
using UnityEngine;

namespace Scrabby.Networking
{
    public class GpsPublisher : MonoBehaviour
    {
        private JObject _gpsData;

        private float _gpsFlowRate;
        private float _gpsLatNoise;
        private float _gpsLonNoise;

        private string _gpsTopic;
        private string _gpsType;
        private string _gpsLatField;
        private string _gpsLonField;

        private float _nextPublishTime;

        private void Start()
        {
            var robot = Robot.Active;
            _gpsFlowRate = robot.GetOption("topics.gps.rate", 2.0f);
            _gpsLatNoise = robot.GetOption("topics.gps.lat_noise", 0.0f);
            _gpsLonNoise = robot.GetOption("topics.gps.lon_noise", 0.0f);

            _gpsType = robot.GetOption("topics.gps.type", "autonav_msgs/GPSFeedback");
            _gpsTopic = robot.GetOption("topics.gps", "/autonav/gps");
            _gpsLatField = robot.GetOption("topics.gps.lat_field", "latitude");
            _gpsLonField = robot.GetOption("topics.gps.lon_field", "longitude");

            _gpsData = new JObject();
        }

        private void FixedUpdate()
        {
            if (Time.time < _nextPublishTime) return;

            _nextPublishTime = Time.time + 1.0f / _gpsFlowRate;
            var pos = transform.position;
            var length = Map.Active.originLength;
            var origin = Map.Active.origin;
            _gpsData[_gpsLatField] = (pos.z + Utilities.Math.GetRandomNormal(0, _gpsLatNoise)) / length.x + origin.x;
            _gpsData[_gpsLonField] = (pos.x + Utilities.Math.GetRandomNormal(0, _gpsLonNoise)) / length.y + origin.y;
            // _gpsData["altitude"] = 0.0f;
            // _gpsData["gps_fix"] = 0;
            // _gpsData["is_locked"] = false;
            // _gpsData["satellites"] = 0;
            // Debug.Log($"Publishing GPS: {_gpsData}");
            Network.Instance.Publish(_gpsTopic, _gpsType, _gpsData);
        }
    }
}