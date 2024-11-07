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

    // for manual control, you don't want to return the wheels to 0 even if you're not actively pressing the right arrow, the robot should coast along its current path
    float drive = 0.0f;
    float strafe = 0.0f;
    float steer = 0.0f;

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

            // basically the robot should coast instead of trying to reset the wheels to 0 upon not getting something on a certain axis
            // TODO: see https://github.com/Team-OKC-Robotics/FRC-2023/blob/master/src/main/cpp/subsystems/SwerveDrive.cpp line 155 VectorTeleOpDrive()
           
            drive = Input.GetAxis("Vertical");
            strafe = Input.GetAxis("Horizontal");
            steer = Input.GetAxis("Horizontal2");

           // if any of the inputs is being pressed, then steer the wheels (because otherwise we don't want to move the wheels back to 0 degrees rotation)
           if (Mathf.Abs(drive) > 0.05 || Mathf.Abs(strafe) > 0.05 || Mathf.Abs(steer) > 0.05) {
                drivetrain.Drive(drive * -500f, strafe * -500f, steer * 100f, true);
            // if everything is less than 0 then set everything to 0
           } else if (Mathf.Abs(drive) < 0.05 && Mathf.Abs(strafe) < 0.05 && Mathf.Abs(steer) < 0.05) {
                drivetrain.Drive(0.0f, 0.0f, 0.0f, false);
           // otherwise, we don't want to steer th wheels
           } else {
                drivetrain.Drive(drive * -500f, strafe * -500f, steer * 100f, false);
           }

        } else {
            //TODO shouldn't we not make new variables every loop, split across the if statement?
            float drive = motorInputMsg.forward_velocity;
            float strafe = motorInputMsg.sideways_velocity;
            float steer = motorInputMsg.angular_velocity;

            drivetrain.Drive(drive*-100f, strafe*-100f, steer*100f, true); //FIXME
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