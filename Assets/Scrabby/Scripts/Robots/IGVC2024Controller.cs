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

        public float forwardControl;
        public float sidewaysControl;
        public float angularControl;
        private ROSConnection _ros;

        public GameObject safeteyLight;
        private Material _safetyLightMaterial;
        private float _nextBlinkTime = 0f;
        public float safetyLightBlinkRate = 1f;
        private int _safetyState = 0;
        private bool _lightOn = false;
        private Color _safetyLightColor = Color.white;

        private Vector3 _lastPosition;
        private Vector3 _lastRotation;

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

                localVelocity = new Vector2(inputX, inputY) * 2;
                angularInput = inputRot;
            }
            else
            {
                localVelocity = new Vector2(sidewaysControl, forwardControl);
                angularInput = angularControl;
            }

            // Convert angularInput to be in radians per second
            angularInput *= Mathf.Rad2Deg;

            // Store the current position and rotation
            Vector3 currentPosition = transform.position;
            Vector3 currentRotation = transform.eulerAngles;

            // Move the robot (Translate and Rotate)
            transform.Translate(localVelocity.x * Time.deltaTime, 0, localVelocity.y * Time.deltaTime);
            transform.Rotate(0, angularInput * Time.deltaTime, 0);

            // Calculate deltas
            float deltaX = currentPosition.x - _lastPosition.x;
            float deltaY = currentPosition.z - _lastPosition.z;
            float deltaTheta = currentRotation.y - _lastRotation.y;

            MotorFeedbackMsg msg = new()
            {
                delta_x = deltaY,
                delta_y = -deltaX,
                delta_theta = -deltaTheta * Mathf.Deg2Rad,
            };
            _ros.Publish("/autonav/motor_feedback", msg);

            // Update the last position and rotation
            _lastPosition = currentPosition;
            _lastRotation = currentRotation;
        }
    }
}
