using UnityEngine;


enum ModulePosition {
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

    private ModulePosition pos;

    private const float wheelRadius = 0.2032f; // TODO in Unity the wheel radius is 4 but in CAD the wheel radius is 8 inches, so what do we use here? Fundamental problem is trying to scale everything in Unity right...
    
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

    public void SetSetpoints(float wheelVel, float steerVel) {
        drivePID.SetSetpoint(wheelVel);
        steerPID.SetSetpoint(steerVel);
    }

    public float GetWheelVelocity() {
        // https://docs.unity3d.com/6000.0/Documentation/ScriptReference/WheelCollider.html
        return 2 * Mathf.PI * wheelRadius * wheelCollider.rpm;
    }

    public float GetAngle() {
        //TODO
        return wheelCollider.transform.rotation.eulerAngles.y;
    }

    // Update is called once per frame
    void Update()
    {
        wheelCollider.GetWorldPose(out position, out rotation);
        wheelModel.transform.position = position;
        wheelModel.transform.rotation = rotation;

        //TODO
        wheelCollider.motorTorque = Mathf.Clamp(drivePID.Calculate(GetWheelVelocity()), -1000, 1000);
        // wheelCollider.steerAngle += Time.fixedDeltaTime * steerPID.Calculate(Mathf.Clamp(GetAngle(), -5, 5)); //TODO this needs to be able to handle angle wrapping and like have torque and stuff
        wheelCollider.steerAngle = 0.0f;
    }
}
