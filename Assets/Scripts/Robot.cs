using UnityEngine;
using Unity.Robotics.ROSTCPConnector;
using RosMessageTypes.Autonav;

public class Robot : MonoBehaviour
{
    private ROSConnection ros;
    private SwerveDrive drivetrain;
    private GameObject thirdPersonCamera;
    private bool connected;

    private MotorInputMsg motorInputMsg;

    // TODO make these configurable
    private string motorFeedbackTopicName = "/autonav/motor_feedback";
    private string motorInputTopicName = "/autonav/motor_input";

    private float timeElapsed = 0.0f;
    private float motorFeedbackFrequency = 0.5f; // twice a second TODO make real-life-accurate

    // for manual control, you don't want to return the wheels to 0 even if you're not actively pressing the right arrow, the robot should coast along its current path
    private float drive = 0.0f;
    private float strafe = 0.0f;
    private float steer = 0.0f;

    // mouse-controlled orbit camera spherical coordinates stuff
    private float theta = 0.0f; // controls azimuth
    private float phi = 0.0f; // controls altitude
    private float rho = Mathf.Sqrt((-15)*(-15) + (57)*57 + 47*47); //FIXME this should be configurable and not hard-coded
    private Vector3 mousePos = new Vector3(0, 0, 0);
    private const float mouseScaleFactor = 100f; //TODO tune this, configurable idk

    private Vector3 initialPosition = new Vector3(47f, .6f, 26f);
    private Vector3 initialHeading = new Vector3(0, 90, 0);

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        //TODO subscribers/publishers
        drivetrain = GetComponent<SwerveDrive>();

        thirdPersonCamera = GameObject.Find("thirdPersonCamera");

        ros = ROSConnection.GetOrCreateInstance();

        ros.RegisterPublisher<MotorFeedbackMsg>(motorFeedbackTopicName);

        ros.Subscribe<MotorInputMsg>(motorInputTopicName, MotorInputCallback);
    }

    // void Update() {
    void FixedUpdate() // probably should have this be fixed update because it's physics related?
    {
        // if game is paused
        if (SettingsManager.paused) {
            // do not register input
            // do not send commands to the swerve drive
            // and do not publish messages
            return;
        }

        if (SettingsManager.needToSetPosition) {
            transform.position = initialPosition + new Vector3(0, 0, SettingsManager.positionOffset);
            transform.eulerAngles = initialHeading + new Vector3(0, SettingsManager.initialHeading, 0);

            SettingsManager.needToSetPosition = false;
        }

        // actually drive
        if (SettingsManager.manualEnabled) {
            //TODO should this be field-oriented? or like, if the 3rd person camera is stationary (doesn't rotate with robot) then camera oriented? idk.

            // basically the robot should coast instead of trying to reset the wheels to 0 upon not getting something on a certain axis
            // TODO: see https://github.com/Team-OKC-Robotics/FRC-2023/blob/master/src/main/cpp/subsystems/SwerveDrive.cpp line 155 VectorTeleOpDrive()
           
            drive = Input.GetAxis("Vertical");
            strafe = Input.GetAxis("Horizontal");
            steer = Input.GetAxis("Steer");

           // if any of the inputs is being pressed, then steer the wheels (because otherwise we don't want to move the wheels back to 0 degrees rotation)
           if (Mathf.Abs(drive) > 0.05 || Mathf.Abs(strafe) > 0.05 || Mathf.Abs(steer) > 0.05) {
                drivetrain.Drive(drive * -100f, strafe * -100f, steer * 100f, true);
            // if everything is less than 0 then set everything to 0
           } else if (Mathf.Abs(drive) < 0.05 && Mathf.Abs(strafe) < 0.05 && Mathf.Abs(steer) < 0.05) {
                drivetrain.Drive(0.0f, 0.0f, 0.0f, false);
           // otherwise, we don't want to steer the wheels
           } else {
                drivetrain.Drive(drive * -700f, strafe * -700f, steer * 100f, false);
           }

        } else {
            if (motorInputMsg != null) {
                //TODO shouldn't we not make new variables every loop, split across the if statement?
                drive = motorInputMsg.forward_velocity;
                strafe = motorInputMsg.sideways_velocity;
                steer = motorInputMsg.angular_velocity;

                drivetrain.Drive(drive*-100f, strafe*-100f, steer*100f, true); //FIXME
            }
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

        // check for camera view stuff
        switch(SettingsManager.cameraView) {
            case "fixed":
                // place camera in starting configuration behind and above the robot
                thirdPersonCamera.transform.localPosition = new Vector3(-15, 57, 47); //TODO make this configurable or zommable or something?

                // and face it forwards
                thirdPersonCamera.transform.localEulerAngles = new Vector3(30, 180, 0); //FIXME
                break;
            case "mouse":
                UnityEngine.Cursor.lockState = CursorLockMode.Locked; // https://docs.unity3d.com/6000.0/Documentation/ScriptReference/Cursor-lockState.html
                // spherical coordinates r(rho, theta, phi) = <rho*cos(theta)sin(phi), rho*sin(theta)sin(phi), rho*cos(phi)>
                // where rho stays constant at whatever dist(-15, 57, 47) (from "fixed" view) is, and theta and phi are controlled by the mouse, going [0, 2pi] and [0, pi] respectively
                // phi = 0 is directly at the north pole, phi=pi/2 is at the equator
                // theta = 0=2pi is at the front of the robot, theta = pi is at the back???
                // although maybe we don't want them to go below the ground? also, what about barrels? do we clip through barrels? are we allowing zoom in and out?
                // I don't want to, but it might be cool
                theta += Input.mousePositionDelta.x / mouseScaleFactor;
                phi += Input.mousePositionDelta.y / mouseScaleFactor;
                
                theta = Mathf.Clamp(theta, 0.0f, 2*Mathf.PI);
                phi = Mathf.Clamp(phi, 0.0f, Mathf.PI);
                // phi %= Mathf.PI; // keep phi between [0, pi]
                // theta %= 2*Mathf.PI; // keep theta between [0, 2pi]
                
                thirdPersonCamera.transform.localPosition = new Vector3(rho*Mathf.Cos(theta)*Mathf.Sin(phi), rho*Mathf.Sin(theta)*Mathf.Sin(phi), rho*Mathf.Cos(phi));

                // not sure how to calculate the rotation the camera should be at. probably involes trig. at phi=0 the camera should be tilted down by 90 degrees. at phi = pi/2 the camera should be level with the horizon
                // at theta = 0 the camera should be 180 degrees (facing the robot), and theta = pi the camera should be facing the back of the robot (in the forwards direction)
                thirdPersonCamera.transform.localEulerAngles = new Vector3(Mathf.Rad2Deg*Mathf.Cos(theta), 0, Mathf.Rad2Deg*Mathf.Sin(theta)); //TODO

                break;
            case "auto":
                //TODO make camera like that one trailmakers camera
                break;
            case "bird's eye": //TODO this should probably put us in field-oriented mode, no?
                // place camera above the map
                thirdPersonCamera.transform.position = new Vector3(40, 140, 0);

                // and face it downwards
                thirdPersonCamera.transform.eulerAngles = new Vector3(90, 0, 90);
                // this is somewhat shaky for some reason, in the future probably best to implement as a separate camera and just sitch to it while disabling original 3rd person
                // when its not rendering and then disabling this camera and going back to the 3rd person one whenver we swap to like fixed for instance
             break;
            case "follow":
                //TODO make camera like scrap mechanic follow
                break;
            case "cinematic":
                //TODO this is for issue #11, not our problem right now
                break;
            default: // default is fixed camera view
                SettingsManager.cameraView = "fixed";
                break;
        }
    }

    public void MotorInputCallback(MotorInputMsg msg) {
        motorInputMsg = msg;
    }
}