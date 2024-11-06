using UnityEngine;


public enum ModulePosition {
    FrontLeft,
    FrontRight,
    BackLeft,
    BackRight
};

// for reference (but don't copy it's bad code)
// https://github.com/Team-OKC-Robotics/FRC-2023/blob/master/src/main/cpp/SwerveModule.cpp

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

    public ModulePosition pos;

    private const float wheelRadius = 0.2032f; // TODO in Unity the wheel radius is 4 but in CAD the wheel radius is 8 inches, so what do we use here? Fundamental problem is trying to scale everything in Unity right...

    private float lastAngle = 0.0f;
    private float maxAllowedAngleChange = 5.0f;
    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        drivePID = new PIDController();
        steerPID = new PIDController();

        drivePID.SetConstants(200f, 0.0f, 0.0f); //TODO
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

    public void SetSetpoints(float wheelVel, float steerAngle) {
        drivePID.SetSetpoint(wheelVel);
        steerPID.SetSetpoint(steerAngle);

        //FIXME this is supposed to be the not-rotate-more-than 90 degrees logic, not sure if it works or not tho
        if (Mathf.Abs(wheelCollider.steerAngle - Mathf.Abs(steerPID.GetSetpoint())) > 100) {
            steerPID.SetSetpoint(steerAngle-180);
            drivePID.SetSetpoint(-drivePID.GetSetpoint());
        } 

        // steerPID.SetSetpoint(steerAngle);
        // steerPID.SetSetpoint(Mathf.Clamp(steerAngle, -90, 90));
    }

    public float GetWheelVelocity() {
        // https://docs.unity3d.com/6000.0/Documentation/ScriptReference/WheelCollider.html
        return 2 * Mathf.PI * wheelRadius * wheelCollider.rpm;
    }

    public float GetAngle() {
        //TODO
        return wheelCollider.transform.rotation.eulerAngles.y;
    }

    // void Update()
    void FixedUpdate() // probably should have this be fixed update because it's physics related?
    {
        wheelCollider.GetWorldPose(out position, out rotation);
        wheelModel.transform.position = position;
        wheelModel.transform.rotation = rotation;

        //TODO
        wheelCollider.motorTorque = Mathf.Clamp(drivePID.Calculate(GetWheelVelocity()), -1000, 1000);
        wheelCollider.steerAngle = Mathf.Clamp(steerPID.GetSetpoint(), wheelCollider.steerAngle-maxAllowedAngleChange, wheelCollider.steerAngle+maxAllowedAngleChange);
    }
}