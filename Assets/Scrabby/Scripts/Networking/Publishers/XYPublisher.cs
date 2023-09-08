using Newtonsoft.Json.Linq;
using Scrabby.ScriptableObjects;
using UnityEngine;

namespace Scrabby.Networking.Publishers
{
    public class XYPublisher : MonoBehaviour
    {
        private JObject _xyData;
        private float _nextPublishTime;

        private void Start()
        {
            _xyData = new JObject();
        }

        private void FixedUpdate()
        {
            if (Time.time < _nextPublishTime) return;

            _nextPublishTime = Time.time + 1.0f / 2.0f;
            var t = transform.position;
            _xyData["x"] = t.x;
            _xyData["y"] = t.z;
            Network.instance.Publish("/onboarding/position", "geometry_msgs/Point", _xyData);
        }
    }
}