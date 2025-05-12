using RosMessageTypes.Autonav;
using Scrabby.ScriptableObjects;
using Unity.Robotics.ROSTCPConnector;
using UnityEngine;

namespace Scrabby.Networking.Publishers
{
    public class GpsPublisher : MonoBehaviour
    {
        [Header("Settings")]
        public float frequency = 0.25f;
        public string topic = "/autonav/gps";
        public float latitudeNoise;
        public float longitudeNoise;
        

        // Private variables
        private float _timeElapsed;
        private ROSConnection _ros;

        void Start()
        {
            _ros = ROSConnection.GetOrCreateInstance();
            _ros.RegisterPublisher<GPSFeedbackMsg>(topic);
        }

        private void FixedUpdate()
        {
            _timeElapsed += Time.fixedDeltaTime;
            if (_timeElapsed > frequency && Map.Active != null)
            {
                Map map = Map.Active;
                // Publish the GPS data
                GPSFeedbackMsg msg = new()
                {
                    latitude = (transform.position.z + Utilities.Math.GetRandomNormal(0, latitudeNoise)) / map.originLength.x + map.origin.x,
                    longitude = (transform.position.x + Utilities.Math.GetRandomNormal(0, longitudeNoise)) / map.originLength.y + map.origin.y,
                    altitude = 0.0f,
                    gps_fix = 3,
                    num_satellites = 7
                };
                _ros.Publish(topic, msg);

                // Reset the elapsed time
                _timeElapsed = 0.0f;
            }
        }
    }
}