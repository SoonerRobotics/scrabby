using UnityEngine;

// truly a classic
// https://github.com/wpilibsuite/allwpilib/blob/main/wpimath/src/main/java/edu/wpi/first/math/controller/PIDController.java
public class PIDController : MonoBehaviour
{
    private float kP;
    private float kI;
    private float kD;

    private float setpoint = 0f;
    private float error = 0f;
    private float lastError = 0f;

    private float lastTime;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        //TODO
        lastTime = -1;
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
        float output = 0.0;
        float e = setpoint - reading;
        float dt = time.now() - lastTime;

        if (lastTime == -1) {
            lastTime = time.now();
            return 0;
        }

        totalError += e * dt;

        output += e * kP;
        output += totalError * kI;
        output += (error - lastError) / dt  *  kD;

        lastTime = time.now();

        return output;
    }

    public void Reset() {
        lastTime = -1;
        totalError = 0.0;
        setpoint = 0.0;
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
