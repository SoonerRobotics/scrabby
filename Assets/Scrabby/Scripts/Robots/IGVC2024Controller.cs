using Newtonsoft.Json.Linq;
using RosMessageTypes.Autonav;
using Scrabby.Interface;
using Scrabby.ScriptableObjects;
using Scrabby.State;
using Unity.Robotics.ROSTCPConnector;
using UnityEngine;

namespace Scrabby.Robots
{
    public class Igvc2024Controller : BaseRobot
    {
        public Robot robot;

        public float speedMod = 2.2727273f;
        public float wheelRadius = 0.1016f;
        public float distanceBetweenWheels = 0.3f;
        public float axleLength = 0.492f;
        public float minSpeed = 0.1f;
        public float drag = 0.85f;

        public float forwardControl;
        public float sidewaysControl;
        public float angularControl;
        private ROSConnection _ros;

        public float feedbackFrequency = 0.1f;
        private float _lastFeedbackTime = 0f;

        private Vector2[] wheelPositions;    // Local positions of wheels relative to center
        private Vector2[] wheelVelocities;   // Calculated velocities per wheel

        public GameObject safeteyLight;
        private Material _safetyLightMaterial;
        private float _nextBlinkTime = 0f;
        public float safetyLightBlinkRate = 1f;
        private int _safetyState = 0;
        private bool _lightOn = false;
        private Color _safetyLightColor = Color.white;

        private void Start()
        {
            _ros = ROSConnection.GetOrCreateInstance();
            _ros.RegisterPublisher<MotorFeedbackMsg>("/autonav/motor_feedback");
            _ros.Subscribe<MotorInputMsg>("/autonav/motor_input", OnMotorInputReceived);
            _ros.Subscribe<SafetyLightsMsg>("/autonav/safety_lights", OnSafetyLightsReceived);

            // Setup safety light material
            if (safeteyLight != null)
            {
                _safetyLightMaterial = safeteyLight.GetComponent<Renderer>().material;
            }

            // Setup swerve drive geometry
            float halfLength = axleLength / 2f;
            float halfWidth = distanceBetweenWheels / 2f;

            wheelPositions = new Vector2[4];
            wheelVelocities = new Vector2[4];

            // Order: FrontLeft, FrontRight, BackLeft, BackRight
            wheelPositions[0] = new Vector2(-halfWidth, halfLength);
            wheelPositions[1] = new Vector2(halfWidth, halfLength);
            wheelPositions[2] = new Vector2(-halfWidth, -halfLength);
            wheelPositions[3] = new Vector2(halfWidth, -halfLength);

            InitializeRobot(robot.options);
        }

        private void OnMotorInputReceived(MotorInputMsg msg)
        {
            forwardControl = msg.forward_velocity;
            sidewaysControl = -msg.sideways_velocity;
            angularControl = msg.angular_velocity;

            if ((Mathf.Abs(forwardControl) > 0 || Mathf.Abs(sidewaysControl) > 0 || Mathf.Abs(angularControl) > 0) && !RunTimer.Instance.IsStarted)
            {
                RunTimer.Instance.Begin();
            }
        }

        private void OnSafetyLightsReceived(SafetyLightsMsg msg)
        {
            // Update emission color based on safety light status
            Color color = new(msg.red, msg.green, msg.blue);
            if (_safetyLightMaterial != null)
            {
                _safetyLightColor = color;
                _safetyLightMaterial.SetColor("_EmissionColor", color);
            }

            _safetyState = msg.mode;
        }

        protected override void RobotUpdate()
        {
            // Safety light blinking logic
            if (safeteyLight != null)
            {
                if (_safetyState == 1)
                {
                    if (Time.time >= _nextBlinkTime)
                    {
                        _lightOn = !_lightOn;
                        _safetyLightMaterial.SetColor("_EmissionColor", _lightOn ? _safetyLightColor : Color.black);
                        _nextBlinkTime = Time.time + safetyLightBlinkRate;
                    }
                }
            }

            if (!CanMove())
            {
                return;
            }

            Vector2 localVelocity;
            float angularInput;

            if (ScrabbyState.Instance.canMoveManually)
            {
                float inputX = Input.GetAxis("Horizontal"); // A/D or left stick X
                float inputY = Input.GetAxis("Vertical");   // W/S or left stick Y
                float inputRot = Input.GetAxis("Rotate");   // Q/E or right stick X

                localVelocity = new Vector2(inputX, inputY) * speedMod;
                angularInput = inputRot;

                // Save to network-style variables (optional telemetry/debugging)
                forwardControl = localVelocity.y;
                sidewaysControl = localVelocity.x;
                angularControl = angularInput;
            }
            else
            {
                localVelocity = new Vector2(sidewaysControl, forwardControl);
                angularInput = angularControl;
            }

            // === Modified: local velocity is already robot-relative, no world rotation needed ===
            Vector2 linearVelocity = localVelocity;

            // Calculate wheel velocities (linear + rotational)
            Vector2 averageVelocity = Vector2.zero;
            for (int i = 0; i < 4; i++)
            {
                Vector2 pos = wheelPositions[i];
                Vector2 rotationalVelocity = new Vector2(-angularInput * pos.y, angularInput * pos.x);
                wheelVelocities[i] = linearVelocity + rotationalVelocity;
                averageVelocity += wheelVelocities[i];
            }
            averageVelocity /= 4f;

            Vector3 movement = new Vector3(linearVelocity.x, 0, linearVelocity.y);

            // === Modified: use Space.Self for robot-relative movement ===
            transform.Translate(movement * Time.fixedDeltaTime, Space.Self);
            transform.Rotate(0, angularInput * Mathf.Rad2Deg * Time.fixedDeltaTime, 0, Space.World);

            PublishFeedback(averageVelocity);
        }

        private void PublishFeedback(Vector2 avgVelocity)
        {
            float deltaT = Time.fixedDeltaTime;
            float deltaX = avgVelocity.y * deltaT;
            float deltaY = -avgVelocity.x * deltaT;
            float deltaTheta = angularControl * deltaT;

            MotorFeedbackMsg msg = new()
            {
                delta_x = deltaX,
                delta_y = deltaY,
                delta_theta = deltaTheta
            };
            _ros.Publish("/autonav/motor_feedback", msg);
        }
    }
}
