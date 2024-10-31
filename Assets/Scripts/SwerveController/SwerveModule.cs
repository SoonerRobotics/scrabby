using UnityEngine;

// for reference (but don't copy it's bad code)
// https://github.com/Team-OKC-Robotics/FRC-2023/blob/master/src/main/cpp/SwerveModule.cpp
public class SwerveModule : MonoBehaviour
{
    // man I feel like I'm writing FRC code all over again.
    PIDController drivePID;
    PIDController steerPID;

    //TODO
    Motor driveMotor;
    Motor steerMotor;

    RelativeEncoder driveEncoder;
    AbsoluteEncoder steerEncoder;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        drivePID = new PIDController(0.1, 0, 0); //TODO
        steerPID = new PIDController(0.1, 0, 0); //TODO

        drivePID.reset();
        steerPID.reset();
    }

    //TODO
    void OnReset() {
        drivePID.reset();
        steerPID.reset();
    }

    // Update is called once per frame
    void Update()
    {

        driveMotor.set(drivePID.calculate(driveEncoder.getVelocity()));
        steerMotor.set(steerPID.calculate(steerEncoder.getPosition())); //TODO this needs to be able to handle angle wrapping
    }
}
