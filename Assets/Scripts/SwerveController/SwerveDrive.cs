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

    public Transform chassisModel;

    private float drive = 0.0f; // FIXME make private once debugged
    private float strafe = 0.0f;
    private float steer = 0.0f;

    private float drive_ = 0.0f; // FIXME make private once debugged
    private float strafe_ = 0.0f;
    private float steer_ = 0.0f;

    private bool allowSteer = true;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        // Find all child GameObjects that have the SwerveModule script attached
        modules = GetComponentsInChildren<SwerveModule>();
        
        frontLeftModule = modules[0];
        frontRightModule = modules[1];
        backLeftModule = modules[2];
        backRightModule = modules[3];
    }

    public void Drive(float drive_, float strafe_, float steer_) {
        // another classic FRC link https://www.chiefdelphi.com/t/paper-4-wheel-independent-drive-independent-steering-swerve/107383
        // and some *really* bad code: https://github.com/Team-OKC-Robotics/FRC-2023/blob/master/src/main/cpp/subsystems/SwerveDrive.cpp

        // do field oriented drive stuff
        if (SettingsManager.fieldOriented) {
            // https://github.com/Team-OKC-Robotics/FRC-2023/blob/master/src/main/cpp/subsystems/SwerveDrive.cpp
            drive = drive_ * Mathf.Cos(Mathf.Deg2Rad * (transform.eulerAngles[1] - 90))  +  strafe_ * Mathf.Sin(Mathf.Deg2Rad * (transform.eulerAngles[1] - 90));
            strafe = strafe_ * Mathf.Cos(Mathf.Deg2Rad * (transform.eulerAngles[1] - 90))  -  drive_ * Mathf.Sin(Mathf.Deg2Rad * (transform.eulerAngles[1] - 90));
        } else {
            drive = drive_;
            strafe = strafe_;
        }

        // if any of the inputs is being pressed, then steer the wheels (because otherwise we don't want to move the wheels back to 0 degrees rotation)
        if (Mathf.Abs(drive_) > 0.05 || Mathf.Abs(strafe_) > 0.05 || Mathf.Abs(steer_) > 0.05) {
            steer = steer_; // then allow the steer
            allowSteer = true;
        // if all inputs are close to 0, then don't pass any input in, let the robot coast, and don't try to steer the wheels
        } else if (Mathf.Abs(drive_) < 0.05 && Mathf.Abs(strafe_) < 0.05 && Mathf.Abs(steer_) < 0.05) {
            steer = 0; // no steer for u
            allowSteer = false;
        }

        // scale the inputs (Unity stick/keyboard values should be in the range [-3, 3] or something I think)
        drive *= 100;
        strafe *= 100;
        steer *= 100;


        float A = strafe - (steer * halfWheelbase);
        float B = strafe + (steer * halfWheelbase);
        float C = drive - (steer * halfTrackwidth);
        float D = drive + (steer * halfTrackwidth);

        //TODO make the modules never turn more than 90 degrees thing (they currently do that in SwerveModule.cs)

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
}
