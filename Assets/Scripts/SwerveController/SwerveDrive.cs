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

    Rigidbody rigidBody;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        // https://docs.unity3d.com/6000.0/Documentation/Manual/WheelColliderTutorial.html
        // rigidBody = GetComponent<Rigidbody>();

        // Adjust center of mass vertically, to help prevent the robot from rolling
        // rigidBody.centerOfMass += Vector3.up * 1;


        // Find all child GameObjects that have the SwerveModule script attached
        modules = GetComponentsInChildren<SwerveModule>();
        
        frontLeftModule = modules[0];
        frontRightModule = modules[1];
        backLeftModule = modules[2];
        backRightModule = modules[3];
    }

    public void Drive(float forward_vel, float sideways_vel, float theta_vel) {
        // another classic FRC link https://www.chiefdelphi.com/t/paper-4-wheel-independent-drive-independent-steering-swerve/107383
        // and some *really* bad code: https://github.com/Team-OKC-Robotics/FRC-2023/blob/master/src/main/cpp/subsystems/SwerveDrive.cpp

        float A = sideways_vel - (theta_vel * halfWheelbase);
        float B = sideways_vel + (theta_vel * halfWheelbase);
        float C = forward_vel - (theta_vel * halfTrackwidth);
        float D = forward_vel + (theta_vel * halfTrackwidth);

        //TODO make the modules never turn more than 90 degrees thing

        frontLeftModule.SetSetpoints(Mathf.Sqrt(B*B + D*D), Mathf.Atan2(B, D) * 180/Mathf.PI);
        frontRightModule.SetSetpoints(Mathf.Sqrt(B*B + C*C), Mathf.Atan2(B, C) * 180/Mathf.PI);
        backLeftModule.SetSetpoints(Mathf.Sqrt(A*A + D*D), Mathf.Atan2(A, D) * 180/Mathf.PI);
        backRightModule.SetSetpoints(Mathf.Sqrt(A*A + C*C), Mathf.Atan2(A, C) * 180/Mathf.PI);
    }

    // Update is called once per frame
    void Update()
    {

    }
}
