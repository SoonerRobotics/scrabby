using UnityEngine;

public class SwerveDrive : MonoBehaviour
{
    private SwerveModule frontLeftModule;
    private SwerveModule frontRightModule;
    private SwerveModule backLeftModule;
    private SwerveModule backRightModule;

    private SwerveModule[] modules;

    // everything here should be in meters
    private const float wheelbase = 0.6096f; // measurements are from Micah's CAD, unsure of accuracy
    private const float trackwidth = 0.6096f; // wheelbase is a square for swerve drive, but chassis itself is rectangle to meet minimum size requirements
    private const float halfWheelbase = wheelbase/2;
    private const float halfTrackwidth = trackwidth/2;

    private float lastX = 0.0f;
    private float lastY = 0.0f;
    private float lastTheta = 0.0f;

    public Rigidbody rigidBody;
    public float CogX = 0.0f;
    public float CogY = 0.0f;
    public float CogZ = 0.0f;
    public Transform chassisModel;



    float setpoint = 0.0f;


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        // https://docs.unity3d.com/6000.0/Documentation/Manual/WheelColliderTutorial.html
        rigidBody = GetComponent<Rigidbody>();
        CogX = rigidBody.centerOfMass[0];
        CogY = rigidBody.centerOfMass[1];
        CogZ = rigidBody.centerOfMass[2];

        // Adjust center of mass vertically, to help prevent the robot from rolling
        // rigidBody.centerOfMass += Vector3.up * 1;


        // Find all child GameObjects that have the SwerveModule script attached
        modules = GetComponentsInChildren<SwerveModule>();
        
        frontLeftModule = modules[0];
        frontRightModule = modules[1];
        backLeftModule = modules[2];
        backRightModule = modules[3];
    }

    // /**
    //  * Atan2 but it works for swerve properly or something
    //  * returns units in degrees
    // */
    // public float Atan2Fixed(float y, float x) {
    //     if (x == 0 && y == 0) {
    //         return 0.0f;
    //     }

    //     // assuming y is forwards/backwards, x is strafe left and right
    //     // if x is 0, then we have no strafe component, so we should return 0 turn angle if y is positive and 180 or -180 otherwise
    //     if (x == 0.0f) {
    //         if (y > 0) {
    //             return 0.0f;
    //         } else {
    //             return 180.0f;
    //         }
    //     // if y is 0, then there's no drive component, only strafe, so we should return 90 degrees if x is positive (to the right) and -90 degrees if x is negative (to the left)
    //     } else if (y == 0.0f) {
    //         if (x > 0) {
    //             return 90; // positive steer angles point to the right
    //         } else {
    //             return -90; // and negative steer angles point to the left
    //         }
    //     }

    //     // if y is positive, then steer angles should be between [-90, 90]
    //     if (y > 0) {
    //         if (x < 0) { // if x is less than 0, then we are going straight and strafing left, so [-90, 0]
    //             return Mathf.Atan(x / y) * Mathf.Rad2Deg; // +y and -x so result is negative
    //         } else { // x is greater than 0, so we are going forwards strafing right, so [0, 90]
    //             return Mathf.Atan(x / y) * Mathf.Rad2Deg;
    //         }
    //     } else { // else y is less than 0, so steer angles should be between [-90, -180] U [90, 180]
    //         if (x < 0) { // x is also less than 0, so we're trying to go down and to the right, so [-90, -180]
    //             //FIXME don't think these work, just copypastad them
    //             return Mathf.Atan(x / y) * Mathf.Rad2Deg; // +y and -x so result is negative
    //         } else { // // x is greater than 0, so we're trying to go down and to the right, so [90, 180]
    //             //FIXME don't think these work, just copypastad them
    //             return Mathf.Atan(x / y) * Mathf.Rad2Deg;
    //         }
    //     }
    // }

    public void Drive(float forward_vel, float sideways_vel, float theta_vel, bool allowSteer) {
        // another classic FRC link https://www.chiefdelphi.com/t/paper-4-wheel-independent-drive-independent-steering-swerve/107383
        // and some *really* bad code: https://github.com/Team-OKC-Robotics/FRC-2023/blob/master/src/main/cpp/subsystems/SwerveDrive.cpp

        float A = sideways_vel - (theta_vel * halfWheelbase);
        float B = sideways_vel + (theta_vel * halfWheelbase);
        float C = forward_vel - (theta_vel * halfTrackwidth);
        float D = forward_vel + (theta_vel * halfTrackwidth);

        //TODO make the modules never turn more than 90 degrees thing (they currently do that in SwerveModule.cs)

        // if (setpoint < 270f) {
        //     setpoint += .1f;
        // } else {
        //     setpoint = -270f;
        // }

        // frontLeftModule.SetSetpoints(0.1f, setpoint); //FIXME TEMP BUG XXX HACK
        // frontRightModule.SetSetpoints(0.1f, setpoint); //FIXME TEMP BUG XXX HACK
        // backLeftModule.SetSetpoints(0.1f, setpoint); //FIXME TEMP BUG XXX HACK
        // backRightModule.SetSetpoints(0.1f, setpoint); //FIXME TEMP BUG XXX HACK
        // Debug.Log($"{frontRightModule.wheelCollider.steerAngle}");
        // Debug.Log($"{Atan2Fixed(forward_vel, sideways_vel)}");

        if (allowSteer) {
            frontLeftModule.SetSetpoints(Mathf.Sqrt(A*A + D*D), Mathf.Atan2(A, D) * Mathf.Rad2Deg);
            frontRightModule.SetSetpoints(Mathf.Sqrt(B*B + C*C), Mathf.Atan2(B, C) * Mathf.Rad2Deg);
            backLeftModule.SetSetpoints(Mathf.Sqrt(B*B + D*D), Mathf.Atan2(B, D) * Mathf.Rad2Deg);
            backRightModule.SetSetpoints(Mathf.Sqrt(A*A + C*C), Mathf.Atan2(A, C) * Mathf.Rad2Deg);
        } else {
            frontLeftModule.SetSetpoints(Mathf.Sqrt(B*B + D*D), frontLeftModule.GetSteerSetpoint());
            frontRightModule.SetSetpoints(Mathf.Sqrt(B*B + C*C), frontRightModule.GetSteerSetpoint());
            backLeftModule.SetSetpoints(Mathf.Sqrt(A*A + D*D), backLeftModule.GetSteerSetpoint());
            backRightModule.SetSetpoints(Mathf.Sqrt(A*A + C*C), backRightModule.GetSteerSetpoint());
        }
    }

    public float GetDeltaX() {
        float output = lastX - chassisModel.transform.position.x;

        lastX = chassisModel.transform.position.x;
        return output;
    }

    public float GetDeltaY() {
        float output = lastY - chassisModel.transform.position.y;

        lastY = chassisModel.transform.position.y;
        return output;
    }

    public float GetDeltaTheta() {
        //FIXME
        float output = lastTheta - chassisModel.transform.rotation.eulerAngles.y;

        lastTheta = chassisModel.transform.rotation.eulerAngles.y;
        return output;
    }


    // Update is called once per frame
    void Update()
    {

    }
}
