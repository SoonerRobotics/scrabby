using UnityEngine;
using Unity.Robotics.ROSTCPConnector;
using RosMessageTypes.Autonav;

//FIXME this was all just copied and pasted from GPSPublisher. temporary solution because Feelers expects Position messages from the particle filter, but that's not done yet
public class PositionPublisher : MonoBehaviour {
    public ROSConnection ros;
    private string topicName = "/autonav/position";
    private float publishRate = 10f; // TODO make this configurable
    private float lastPublishTime = 0;

    private float latitude = 0.0f;
    private float longitude = 0.0f;

    private float startLat = 42.668136070f;
    private float startLon = -83.21889127f;

    // bottom right corner (first corner) is (lat, lon) => (31.42255, -17.09999)
    // top right corner (second corner) is (lat, lon) => (-96.12039, -29.79432)

    // from autonav_software_2024/data/waypoints_editor.py using ENTRY_POSITION for the winning run from 2024, the starting coordinates of the robot are approximately
    // (42.668136070, -83.21889127)
    // the first turn is at ~(42.66833704, -83.2189487)
    // and the second turn is (42.668830417, -83.2193430)
    // according to waypoint files, the second turn is at (42.66827, -83.21934), which I'm inclined to trust more
    // so then we can calculate our lat lengths and long lengths by taking the length from one point to another in the sim(unity) coordinates and dividing it by that length in irl GPS coordinates
    // in scrabby start pos of the robot is (25.98906, 47.00509)

    private float latLength = Mathf.Abs((25.98906f - -96.12039f) / (42.668136070f - 42.668830417f));
    private float lonLength = Mathf.Abs((47.00509f - -29.79432f) / (-83.21889127f - -83.2193430f));

    void Start() {
        // start the ROS connection
        ros = ROSConnection.GetOrCreateInstance();
        ros.RegisterPublisher<PositionMsg>(topicName);
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
            latitude = (transform.position.z - 25.98906f) / latLength + startLat;
            longitude = (transform.position.x - 47.00509f) / lonLength + startLon;

            // double altitude = 0.0f; // altitude doesn't matter for us
            // short gps_fix = 3; // see VN200-ICD page 55. Can be [0, 4] U [7, 8]. default of 3 is fine. can change later for testing TODO
            // bool is_locked = true; //TODO make this changable for testing
            // short satellites = 4; //TODO make this changeable for testing
            double x = 0.0f; //FIXME
            double y = 0.0f; //FIXME
            double theta = transform.eulerAngles.y; // might need to be localEulerAngles

            theta *= Mathf.Deg2Rad;

            // GPSFeedbackMsg msg = new GPSFeedbackMsg(latitude, longitude, altitude, gps_fix, is_locked, satellites);
            PositionMsg msg = new PositionMsg(x, y, theta, latitude, longitude);
            ros.Publish(topicName, msg);
            // Debug.Log("PUBLISHING");
        // }
    }
}