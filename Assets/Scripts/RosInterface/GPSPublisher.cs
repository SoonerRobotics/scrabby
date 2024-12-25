using UnityEngine;
using Unity.Robotics.ROSTCPConnector;
using RosMessageTypes.Autonav;

public class GPSPublisher : MonoBehaviour {
    public ROSConnection ros;
    public string topicName = "/autonav/gps/";
    private float publishRate = 1f / 5f; // internal GPS receiver on the VectorNav updates at like 5 Hz according to the ICD or whatever, TODO make this configurable
    private float lastPublishTime = 0;

    public float latitude = 0.0f;
    public float longitude = 0.0f;

    // public Transform robot;

    void Start() {
        // start the ROS connection
        ros = ROSConnection.GetOrCreateInstance();
        ros.RegisterPublisher<GPSFeedbackMsg>(topicName);
    }

    void FixedUpdate() {
        // don't publish if we're paused
        if (SettingsManager.paused) {
            return;
        }

        // publish at the publish rate
        if (Time.time - lastPublishTime < 1f / publishRate) {
            return;
        }

        lastPublishTime = Time.time;

        // don't kill the framerate trying to publish if we haven't established a connection yet
        // if (!ros.HasConnectionError) {
            latitude = (transform.position.z - 25.98906) / 1f; //FIXME (z - 25.98906)
            longitude = (transform.position.x - 47.00509) / 1f; //FIXME (x - 47.00509)

            // bottom right corner (first corner) is (lat, lon) => (31.42255, -17.09999)
            // top right corner (second corner) is (lat, lon) => (-96.12039, -29.79432)

            double altitude = 0.0f; // altitude doesn't matter for us
            short gps_fix = 3; // see VN200-ICD page 55. Can be [0, 4] U [7, 8]. default of 3 is fine. can change later for testing TODO
            bool is_locked = true; //TODO make this changable for testing
            short satellites = 4; //TODO make this changeable for testing

            GPSFeedbackMsg msg = new GPSFeedbackMsg(latitude, longitude, altitude, gps_fix, is_locked, satellites);
            ros.Publish(topicName, msg);
        // }
    }
}