using UnityEngine;

public class Robot : MonoBehaviour
{
    SwerveDrive drivetrain;
    bool connected;
    bool manual = false;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        drivetrain = new SwerveDrive(); //TODO
    }

    // Update is called once per frame
    void Update()
    {
        // another classic FRC link https://www.chiefdelphi.com/t/paper-4-wheel-independent-drive-independent-steering-swerve/107383
        if (manual) {
            //TODO calculate setpoints based on keyboard / controller inputs
        } else {
            //TODO claculate setpoints based on what we get from /autonav/motor_input ROS topic
        }
    }
}
