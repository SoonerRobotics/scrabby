﻿using Scrabby.ScriptableObjects;
using UnityEngine;
using UnityEngine.Rendering;

namespace Scrabby.Networking.Publishers
{
    public class ImagePublisher : MonoBehaviour
    {
        public Camera camera;

        private Texture2D _texture;
        private Rect _rect;

        public int quality = 75;
        public int frameRate = 8;
        public string topic = "/autonav/camera/compressed/left";
        public float lastCaptureTime = 0;

        private int _fps = 8;
        private int _quality = 75;

        public int width = 480;
        public int height = 680;
        public bool flip = false;
        
        private void Start()
        {
            var robot = Robot.Active;
            // _fps = robot.GetOption("topics.camera.fps", 8);
            _fps = 4;
            _quality = robot.GetOption("topics.camera.quality", 75);

            if (flip)
            {
                (width, height) = (height, width);
            }

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
            if (targetCamera != camera)
            {
                return;
            }
            
            if (Time.time - lastCaptureTime < 1f / _fps) return;
            lastCaptureTime = Time.time;

            if (_texture == null)
            {
                return;
            }

            _texture.ReadPixels(_rect, 0, 0);
            var bytes = _texture.EncodeToJPG(_quality);
            RosConnector.Instance.PublishCompressedImage(topic, bytes);
        }
    }
}