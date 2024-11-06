using UnityEngine;
using UnityEngine.Rendering;
using Unity.Robotics.ROSTCPConnector;
using RosMessageTypes.Sensor;
using RosMessageTypes.Std;

// for reference:
// https://github.com/Unity-Technologies/Unity-Robotics-Hub/blob/main/tutorials/ros_unity_integration/unity_scripts/RosPublisherExample.cs
public class ImagePublisher : MonoBehaviour {

    public ROSConnection ros;
    public string topicName = "/autonav/camera/";
    private float frameRate = 30f; // 30 fps, TODO make this configurable
    private float timeElapsed;

    public Camera camera;

    private Texture2D _texture;
    private Rect _rect;

    private float lastCaptureTime = 0;

    private int width = 640;
    private int height = 480;

    void Start() {
        // start the ROS connection
        ros = ROSConnection.GetOrCreateInstance();
        ros.RegisterPublisher<CompressedImageMsg>(topicName);

        _texture = new Texture2D(width, height, TextureFormat.RGB24, false);
        _rect = new Rect(0, 0, width, height);
        camera.targetTexture = new RenderTexture(width, height, 24);
        RenderPipelineManager.endCameraRendering += OnCameraRender;
    }

    private void OnDestroy() {
        RenderPipelineManager.endCameraRendering -= OnCameraRender;
    }

    private void OnCameraRender(ScriptableRenderContext context, Camera targetCamera) {
        if (targetCamera != camera) {
            return;
        }
        
        if (Time.time - lastCaptureTime < 1f / frameRate) {
            return;
        }

        lastCaptureTime = Time.time;

        if (_texture == null) {
            return;
        }

        // don't kill the framerate trying to publish if we haven't established a connection yet
        if (!ros.HasConnectionError) {
            _texture.ReadPixels(_rect, 0, 0);
            var bytes = _texture.EncodeToJPG();
            // https://github.com/Unity-Technologies/ROS-TCP-Connector/blob/main/com.unity.robotics.ros-tcp-connector/Runtime/Messages/Sensor/msg/CompressedImageMsg.cs
            ros.Publish(topicName, new CompressedImageMsg(new HeaderMsg(), "jpeg", bytes)); //TODO give the message a proper Header
        }
    }
}