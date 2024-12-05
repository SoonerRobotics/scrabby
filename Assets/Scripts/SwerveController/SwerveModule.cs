using UnityEngine;

// for reference (but don't copy it's bad code)
// https://github.com/Team-OKC-Robotics/FRC-2023/blob/master/src/main/cpp/SwerveModule.cpp
// https://github.com/Team-OKC-Robotics/FRC-2023/blob/master/src/main/cpp/subsystems/SwerveDrive.cpp

// FRC whitepapers
// https://www.first1684.com/uploads/2/0/1/6/20161347/chimiswerve_whitepaper__2_.pdf
// https://www.chiefdelphi.com/t/paper-4-wheel-independent-drive-independent-steering-swerve/107383/7


// also unity tutorial
// https://docs.unity3d.com/6000.0/Documentation/Manual/WheelColliderTutorial.html
public class SwerveModule : MonoBehaviour
{
    // man I feel like I'm writing FRC code all over again.
    // PID controllers
    private PIDController drivePID;
    private PIDController steerPID;

    // Unity stuff
    public Transform wheelModel;
    public WheelCollider wheelCollider;

    private Vector3 position;
    private Quaternion rotation;

    private const float wheelRadius = 0.2032f; // TODO in Unity the wheel radius is 4 but in CAD the wheel radius is 8 inches, so what do we use here? Fundamental problem is trying to scale everything in Unity right...

    private float maxAllowedAngleChange = 15.0f;
    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        drivePID = new PIDController();
        steerPID = new PIDController();

        drivePID.SetConstants(.5f, 0.0f, 0.0f); //TODO
        steerPID.SetConstants(1.5f, 0.0f, 0.0f); //TODO

        drivePID.Reset();
        steerPID.Reset();

        wheelCollider = GetComponent<WheelCollider>();
    }

    //TODO
    public void OnReset() {
        drivePID.Reset();
        steerPID.Reset();
    }

    public float GetSteerSetpoint() {
        return steerPID.GetSetpoint();
    }

    public void SetSetpoints(float wheelVel, float steerAngle) {
        drivePID.SetSetpoint(wheelVel);
        steerPID.SetSetpoint(WrapAngle(steerAngle));

        // FIXME this is supposed to be the not-rotate-more-than 90 degrees logic, not sure if it works or not tho
        // if (Mathf.Abs(wheelCollider.steerAngle - Mathf.Abs(steerPID.GetSetpoint())) > 100) {
        //     steerPID.SetSetpoint(steerAngle-180);
        //     drivePID.SetSetpoint(-drivePID.GetSetpoint());
        // }
    }

    public float GetWheelVelocity() {
        // https://docs.unity3d.com/6000.0/Documentation/ScriptReference/WheelCollider.html
        return 2 * Mathf.PI * wheelRadius * wheelCollider.rpm;
    }

    // https://github.com/Team-OKC-Robotics/FRC-2023/blob/master/src/main/cpp/Utils.cpp
    // https://github.com/BroncBotz3481/YAGSL/blob/main/swervelib/SwerveModule.java
    public float WrapAngle(float angle) {
        if (angle < -180) {
            return angle + 360;
        } else if (angle > 180) {
            return angle - 360;
        }
        return angle;
    }

    void FixedUpdate() // probably should have this be fixed update because it's physics related?
    {
        if (SettingsManager.paused) {
            return;
        }

        // YAGSL reference, should copy their design https://github.com/BroncBotz3481/YAGSL/blob/main/swervelib/SwerveModule.java
        wheelCollider.GetWorldPose(out position, out rotation);
        wheelModel.transform.position = position;
        wheelModel.transform.rotation = rotation;

        // desiredState = SwerveModuleState.optimize(desiredState, Rotation2d.fromDegrees(getAbsolutePosition()));
        // if (speed < 0.01) {
        //     dontTryToChangeTheAngle();
        // }
        // // Cosine compensation.
        // double velocity = configuration.useCosineCompensator
        //                 ? getCosineCompensatedVelocity(desiredState)
        //                 : desiredState.speedMetersPerSecond;
        // double cosineScalar = 1.0;
        //     // Taken from the CTRE SwerveModule class.
        //     // https://api.ctr-electronics.com/phoenix6/release/java/src-html/com/ctre/phoenix6/mechanisms/swerve/SwerveModule.html#line.46
        //     /* From FRC 900's whitepaper, we add a cosine compensator to the applied drive velocity */
        //     /* To reduce the "skew" that occurs when changing direction */
        //     /* If error is close to 0 rotations, we're already there, so apply full power */
        //     /* If the error is close to 0.25 rotations, then we're 90 degrees, so movement doesn't help us at all */
        //     cosineScalar = Rotation2d.fromDegrees(desiredState.angle.getDegrees())
        //                             .minus(Rotation2d.fromDegrees(getAbsolutePosition()))
        //                             .getCos(); // TODO: Investigate angle modulus by 180.
        //     /* Make sure we don't invert our drive, even though we shouldn't ever target over 90 degrees anyway */
        // if (cosineScalar < 0.0)
        // {
        // cosineScalar = 1;
        // }
        // return desiredState.speedMetersPerSecond * (cosineScalar);

        //TODO
        float motorPower = drivePID.GetSetpoint();
        if (Mathf.Abs(motorPower) < 0.1) {
            motorPower = 0.0f;
            wheelCollider.brakeTorque = 500;
        } else {
            wheelCollider.brakeTorque = 0.0f;
        }
        // wheelCollider.motorTorque = Mathf.Clamp(drivePID.Calculate(GetWheelVelocity()), -1000, 1000);
        wheelCollider.motorTorque = Mathf.Clamp(motorPower, -500, 500);
        wheelCollider.steerAngle = Mathf.Clamp(steerPID.GetSetpoint(), wheelCollider.steerAngle-maxAllowedAngleChange, wheelCollider.steerAngle+maxAllowedAngleChange);
    }
}