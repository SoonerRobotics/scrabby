using RosMessageTypes.Sensor;
using Scrabby.ScriptableObjects;
using Unity.Robotics.ROSTCPConnector;
using UnityEngine;
using UnityEngine.Rendering;

namespace Scrabby.Networking.Publishers
{
    public class ImagePublisher : MonoBehaviour
    {
        [Header("Settings")]
        public float frequency = 0.125f;
        public string topic = "/autonav/camera/compressed/front";
        public int quality = 75;

        [Header("Camera Settings")]
        public Camera camera;
        public int width = 480;
     
        public int height = 680;
        public bool flip = false;

        // Private variables
        private Texture2D _texture;
        private Rect _rect;
        private float _timeElapsed;
        private ROSConnection _ros;
        
        private void Start()
        {
            if (flip)
            {
                (width, height) = (height, width);
            }

            // Ros
            _ros = ROSConnection.GetOrCreateInstance();
            _ros.RegisterPublisher<CompressedImageMsg>(topic);

            // Other stuff
            _texture = new Texture2D(width, height, TextureFormat.RGB24, false);
            _rect = new Rect(0, 0, width, height);
            camera.targetTexture = new RenderTexture(width, height, 24);
            RenderPipelineManager.endCameraRendering += OnCameraRender;
        }

        private void OnDestroy()
        {
            RenderPipelineManager.endCameraRendering -= OnCameraRender;
        }

        private void OnCameraRender(ScriptableRenderContext context, Camera targetCamera)
        {
            if (targetCamera != camera || _texture == null)
            {
                return;
            }
            
            _timeElapsed += Time.deltaTime;
            if (_timeElapsed > frequency)
            {
                _texture.ReadPixels(_rect, 0, 0);
                var bytes = _texture.EncodeToJPG(quality);

                CompressedImageMsg msg = new()
                {
                    data = bytes,
                    format = "jpeg",
                    header = new RosMessageTypes.Std.HeaderMsg()
                };
                _ros.Publish(topic, msg);
                Debug.Log($"Publishing image to {topic}");
                
                _timeElapsed = 0.0f;
            }
        }
    }
}