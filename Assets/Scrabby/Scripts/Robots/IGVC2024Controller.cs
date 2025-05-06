using Newtonsoft.Json.Linq;
using Scrabby.Interface;
using Scrabby.Networking;
using Scrabby.ScriptableObjects;
using Scrabby.State;
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

        private Vector3 LinearVelocity { get; set; }
        private Vector3 AngularVelocity { get; set; }

        private string _inputForwardField;
        private string _inputSidewaysField;
        private string _inputAngularField;

        private string _feedbackTopic;
        private string _feedbackType;
        private string _deltaXField;
        private string _deltaYField;
        private string _deltaThetaField;

        private Vector2[] wheelPositions;    // Local positions of wheels relative to center
        private Vector2[] wheelVelocities;   // Calculated velocities per wheel

        private void Start()
        {
            RosConnector.OnNetworkInstruction.AddListener(OnNetworkInstruction);

            _inputForwardField = robot.GetOption("topics.input.forward", "forward_velocity");
            _inputSidewaysField = robot.GetOption("topics.input.sideways", "sideways_velocity");
            _inputAngularField = robot.GetOption("topics.input.angular", "angular_velocity");

            _feedbackTopic = robot.GetOption("topics.feedback", "/autonav/motor_feedback");
            _feedbackType = robot.GetOption("topics.feedback.type", "autonav_msgs/motor_feedback");
            _deltaXField = robot.GetOption("topics.feedback.deltax", "delta_x");
            _deltaYField = robot.GetOption("topics.feedback.deltay", "delta_y");
            _deltaThetaField = robot.GetOption("topics.feedback.deltatheta", "delta_theta");

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

        private void OnDestroy()
        {
            RosConnector.OnNetworkInstruction.RemoveListener(OnNetworkInstruction);
        }

        private void OnNetworkInstruction(NetworkInstruction instruction)
        {
            Debug.Log($"Received instruction: {instruction.Topic}");
            var inputTopic = "/autonav/motor_input";
            if (inputTopic != instruction.Topic)
            {
                return;
            }

            var forward = instruction.GetData<float>(_inputForwardField);
            var sideways = -instruction.GetData<float>(_inputSidewaysField);
            var angular = instruction.GetData<float>(_inputAngularField);

            Debug.Log($"Forward: {forward}, Sideways: {sideways}, Angular: {angular}");

            forwardControl = forward;
            sidewaysControl = sideways;
            angularControl = angular;

            if ((Mathf.Abs(forward) > 0 || Mathf.Abs(sideways) > 0 || Mathf.Abs(angular) > 0) && !RunTimer.Instance.IsStarted)
            {
                RunTimer.Instance.Begin();
            }
        }

        protected override void RobotUpdate()
        {
            if (!CanMove())
            {
                return;
            }

            float psi = transform.rotation.eulerAngles.y * Mathf.Deg2Rad;

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

            LinearVelocity = movement;
            AngularVelocity = new Vector3(0, angularInput, 0);

            PublishFeedback(averageVelocity);
        }

        private void PublishFeedback(Vector2 avgVelocity)
        {
            float deltaT = Time.fixedDeltaTime;
            float deltaX = avgVelocity.x * deltaT;
            float deltaY = avgVelocity.y * deltaT;
            float deltaTheta = angularControl * deltaT;

            if (_deltaXField == null || _deltaYField == null || _deltaThetaField == null)
            {
                return;
            }

            var data = new JObject
            {
                { _deltaXField, deltaX },
                { _deltaYField, deltaY },
                { _deltaThetaField, deltaTheta }
            };
            RosConnector.Instance.Publish(_feedbackTopic, _feedbackType, data);
        }
    }
}
