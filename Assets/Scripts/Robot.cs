using UnityEngine;
using Unity.Robotics.ROSTCPConnector;
using RosMessageTypes.Autonav;

public class Robot : MonoBehaviour
{
    private ROSConnection ros;
    private SwerveDrive drivetrain;
    private bool connected;
    private bool manual = true;

    private MotorInputMsg motorInputMsg;

    private string motorFeedbackTopicName = "/autonav/motor_feedback";
    private string motorInputTopicName = "/autonav/motor_input";

    private float timeElapsed = 0.0f;
    private float motorFeedbackFrequency = 0.5f; // twice a second TODO make real-life-accurate

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        //TODO subscribers/publishers
        drivetrain = GetComponent<SwerveDrive>();

        ros = ROSConnection.GetOrCreateInstance();

        ros.RegisterPublisher<MotorFeedbackMsg>(motorFeedbackTopicName);

        ros.Subscribe<MotorInputMsg>(motorInputTopicName, MotorInputCallback);
    }

    // void Update() {
    void FixedUpdate() // probably should have this be fixed update because it's physics related?
    {
        // actually drive
        if (manual) {
            //TODO should this be field-oriented? or like, if the 3rd person camera is stationary (doesn't rotate with robot) then camera oriented? idk.

            float drive = Input.GetAxis("Vertical");
            float strafe = Input.GetAxis("Horizontal");
            float steer = Input.GetAxis("Horizontal2");

            strafe = Mathf.Clamp(strafe, -1, 1);

            // deadband
            if (Mathf.Abs(steer) < 0.1) {
                steer = 0.0f;
            }
            if (Mathf.Abs(drive) < 0.1) {
                drive = 0.0f;
            }
            if (Mathf.Abs(strafe) < 0.1) {
                strafe = 0.0f;
            }

            drivetrain.Drive(drive*-100f, strafe*-100f, steer*100f); //FIXME
        } else {
            //TODO shouldn't we not make new variables every loop, split across the if statement?
            float drive = motorInputMsg.forward_velocity;
            float strafe = motorInputMsg.sideways_velocity;
            float steer = motorInputMsg.angular_velocity;

            drivetrain.Drive(drive*-100f, strafe*-100f, steer*100f); //FIXME
        }

        timeElapsed += Time.deltaTime;
        if (timeElapsed > motorFeedbackFrequency) {
            // publish motor feedback message
            var msg = new MotorFeedbackMsg();
            msg.delta_x = drivetrain.GetDeltaX();
            msg.delta_y = drivetrain.GetDeltaY();
            msg.delta_theta = drivetrain.GetDeltaTheta();

            if (!ros.HasConnectionError) {
                ros.Publish(motorFeedbackTopicName, msg);
            }

            timeElapsed = 0f;
        }
    }

    public void MotorInputCallback(MotorInputMsg msg) {
        motorInputMsg = msg;
    }
}