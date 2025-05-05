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
        public float angularControl;

        private Vector3 LinearVelocity { get; set; }
        private Vector3 AngularVelocity { get; set; }

        private string _inputForwardField;
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
            var inputTopic = robot.GetOption("topics.input", "/autonav/MotorInput");
            if (inputTopic != instruction.Topic)
            {
                return;
            }

            var forward = instruction.GetData<float>(_inputForwardField);
            var angular = instruction.GetData<float>(_inputAngularField);
            Debug.Log($"Forward: {forward}, Angular: {angular}");
            forwardControl = forward;
            angularControl = angular;

            if ((forward > 0 || angular > 0) && !RunTimer.Instance.IsStarted)
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

            // Get robot's orientation
            float psi = transform.rotation.eulerAngles.y * Mathf.Deg2Rad;

            // Linear movement vector in world space
            Vector2 linearVelocity;

            if (ScrabbyState.Instance.canMoveManually)
            {
                float inputX = Input.GetAxis("Horizontal"); // A/D or left stick X
                float inputY = Input.GetAxis("Vertical");   // W/S or left stick Y
                float inputRot = Input.GetAxis("Rotate");   // Q/E or right stick X

                // Convert input to world-space direction (using robot's heading)
                Vector3 inputWorld = Quaternion.Euler(0, transform.eulerAngles.y, 0) * new Vector3(inputX, 0, inputY);
                
                // Project into 2D movement vector for swerve
                linearVelocity = new Vector2(inputWorld.x, inputWorld.z) * speedMod;

                forwardControl = linearVelocity.magnitude;
                angularControl = inputRot;
            }
            else
            {
                // Use remote command
                linearVelocity = new Vector2(forwardControl * Mathf.Sin(psi), forwardControl * Mathf.Cos(psi));
            }


            // Calculate per-wheel velocities (linear + rotational)
            Vector2 averageVelocity = Vector2.zero;
            for (int i = 0; i < 4; i++)
            {
                Vector2 pos = wheelPositions[i];
                Vector2 rotationalVelocity = new Vector2(-angularControl * pos.y, angularControl * pos.x);
                wheelVelocities[i] = linearVelocity + rotationalVelocity;
                averageVelocity += wheelVelocities[i];
            }
            averageVelocity /= 4f;

            Vector3 movement = new Vector3(averageVelocity.x, 0, averageVelocity.y);
            transform.Translate(movement * Time.fixedDeltaTime, Space.World);
            transform.Rotate(0, angularControl * Mathf.Rad2Deg * Time.fixedDeltaTime, 0, Space.World);

            // Save linear/angular velocity for possible use
            LinearVelocity = movement;
            AngularVelocity = new Vector3(0, angularControl, 0);

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
