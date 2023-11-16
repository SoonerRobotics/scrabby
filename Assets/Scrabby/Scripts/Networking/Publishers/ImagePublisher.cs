using Scrabby.ScriptableObjects;
using UnityEngine;
using UnityEngine.Rendering;

namespace Scrabby.Networking.Publishers
{
    public class ImagePublisher : MonoBehaviour
    {
        public new Camera camera;

        private Texture2D _texture;
        private Rect _rect;

        private int _quality;
        private int _frameRate;
        private string _topic;
        private float _lastCaptureTime;

        private void Start()
        {
            var robot = Robot.Active;
            var width = robot.GetOption("topics.camera.width", 640);
            var height = robot.GetOption("topics.camera.height", 480);
            _frameRate = robot.GetOption("topics.camera.fps", 8);
            _quality = robot.GetOption("topics.camera.quality", 75);
            _topic = robot.GetOption("topics.camera", "/autonav/camera/compressed");

            _texture = new Texture2D(width, height, TextureFormat.RGB24, false);
            _rect = new Rect(0, 0, width, height);
            camera.targetTexture = new RenderTexture(width, height, 24);
            camera.targetTexture.Create();

            RenderPipelineManager.endCameraRendering += OnCameraRender;
        }

        private void OnDestroy()
        {
            RenderPipelineManager.endCameraRendering -= OnCameraRender;
        }

        private void OnCameraRender(ScriptableRenderContext context, Camera targetCamera)
        {
            if (Time.time - _lastCaptureTime < 1f / _frameRate) return;
            _lastCaptureTime = Time.time;

            if (_texture == null)
            {
                return;
            }

            _texture.ReadPixels(_rect, 0, 0);
            var bytes = _texture.EncodeToJPG(_quality);
            Network.Instance.PublishCompressedImage(_topic, bytes);
        }
    }
}