using UnityEngine;

public class SwerveDrive : MonoBehaviour
{
    SwerveModule frontLeftModule;
    SwerveModule frontRightModule;
    SwerveModule backLeftModule;
    SwerveModule backRightModule;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        frontLeftModule = new SwerveModule(); //TODO
        frontRightModule = new SwerveModule(); //TODO
        backLeftModule = new SwerveModule(); //TODO
        backRightModule = new SwerveModule(); //TODO
    }

    // Update is called once per frame
    void Update()
    {
        frontLeftModule.setSetpoints(driveSetpoint, steerSetpoint);

        //TODO
        driveSetpoint = sqrt(controller.getStickX**2 + controller.getStickY**2);
        steerSetpoint = controller.getRightStickX() || ROS.subscribe("/autonav/motorInput").getLatestMsg().angular_velocity;
    }
}
