using UnityEngine;

public class Robot : MonoBehaviour
{
    private SwerveDrive drivetrain;
    private bool connected;
    private bool manual = true;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        //TODO subscribers/publishers
        drivetrain = GetComponent<SwerveDrive>(); //TODO
    }

    // Update is called once per frame
    void Update()
    {
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

            drivetrain.Drive(drive*-100f, strafe*-100f, steer*100f);
        } else {
            // MotorInput msg = motorInputSubscriber.get();
            // drivetrain.Drive(drive, strafe, steer); //TODO this isn't python this is C#
        }
    }
}