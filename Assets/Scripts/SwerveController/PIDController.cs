using UnityEngine;

// truly a classic
// https://github.com/wpilibsuite/allwpilib/blob/main/wpimath/src/main/java/edu/wpi/first/math/controller/PIDController.java
public class PIDController
{
    private float kP;
    private float kI;
    private float kD;

    private float setpoint = 0f;
    private float error = 0f;
    private float lastError = 0f;
    private float totalError = 0.0f;

    private float lastTime;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        //TODO
        lastTime = -1.0f;
    }

    public void SetSetpoint(float s) {
        this.setpoint = s;
    }

    public void SetConstants(float p, float i, float d) {
        this.kP = p;
        this.kI = i;
        this.kD = d;
    }

    public float Calculate(float reading) {
        float output = 0.0f;
        float e = setpoint - reading;
        float dt = Time.time - lastTime;

        if (lastTime == -1) {
            lastTime = Time.time;
            return 0.0f;
        }

        totalError += e * dt;

        output += e * kP;
        output += totalError * kI;
        output += (error - lastError) / dt  *  kD;

        lastTime = Time.time;

        return output;
    }

    public void Reset() {
        lastTime = -1f;
        totalError = 0.0f;
        setpoint = 0.0f;
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
