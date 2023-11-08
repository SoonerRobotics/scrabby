using Newtonsoft.Json.Linq;
using Scrabby.Networking;
using Scrabby.ScriptableObjects;
using Scrabby.State;
using UnityEngine;
using Network = Scrabby.Networking.Network;

namespace Scrabby.Robots
{
    public class Igvc2022Controller : BaseRobot
    {
        public Robot robot;

        public float speedMod = 2.2727273f;
        public float wheelRadius = 0.1016f;
        public float distanceBetweenWheels = 0.3f;
        public float axleLength = 0.492f;
        public float minSpeed = 0.1f;
        public float drag = 0.85f;

        public float vl, vr; // angular velocities (radians per second)

        public float left, right;

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

        private void Start()
        {
            Network.OnNetworkInstruction += OnNetworkInstruction;

            _inputForwardField = robot.GetOption("topics.input.forward", "forward_velocity");
            _inputAngularField = robot.GetOption("topics.input.angular", "angular_velocity");

            _feedbackTopic = robot.GetOption("topics.feedback", "/scr/motors/feedback");
            _feedbackType = robot.GetOption("topics.feedback.type", "geometry_msgs/Point");
            _deltaXField = robot.GetOption("topics.feedback.deltax", "x");
            _deltaYField = robot.GetOption("topics.feedback.deltay", "y");
            _deltaThetaField = robot.GetOption("topics.feedback.deltatheta", "z");

            InitializeRobot(robot.options);
        }

        private void OnDestroy()
        {
            Network.OnNetworkInstruction -= OnNetworkInstruction;
        }

        private void OnNetworkInstruction(NetworkInstruction instruction)
        {
            var inputTopic = robot.GetOption("topics.input", "/scr/motors/input");
            if (inputTopic != instruction.Topic)
            {
                return;
            }

            var forward = instruction.GetData<float>(_inputForwardField);
            var angular = instruction.GetData<float>(_inputAngularField);
            forwardControl = forward;
            angularControl = angular;
        }

        public void FixedUpdate()
        {
            if (!CanMove())
            {
                return;
            }
            
            var psi = transform.rotation.eulerAngles.y * Mathf.Deg2Rad;
            if (ScrabbyState.Instance.canMoveManually)
            {
                var horiz = Mathf.Pow(Input.GetAxis("Vertical"), 3);
                var vertical = Input.GetAxis("Horizontal");
                left = (horiz * vertical + horiz) * speedMod / wheelRadius;
                right = (-horiz * vertical + horiz) * speedMod / wheelRadius;
            }
            else
            {
                left = (forwardControl / wheelRadius) - (distanceBetweenWheels * (angularControl / wheelRadius));
                right = (forwardControl / wheelRadius) + (distanceBetweenWheels * (angularControl / wheelRadius));
            }

            var psiDot = wheelRadius * (vl - vr) / axleLength;

            if (Mathf.Abs(drag * left) < minSpeed)
            {
                left = 0;
            }

            if (Mathf.Abs(drag * right) < minSpeed)
            {
                right = 0;
            }

            vl = drag * left + (1.0f - drag) * vl;
            vr = drag * right + (1.0f - drag) * vr;

            var dotX = wheelRadius / 2.0f * (vl + vr) * Mathf.Sin(psi);
            var dotY = wheelRadius / 2.0f * (vl + vr) * Mathf.Cos(psi);
            var newLinearVel = new Vector3(dotX, 0, dotY);

            LinearVelocity = newLinearVel;
            AngularVelocity = new Vector3(0, psiDot, 0);
            
            transform.Translate(LinearVelocity * Time.fixedDeltaTime, Space.World);
            transform.Rotate(Time.fixedDeltaTime * Mathf.Rad2Deg * AngularVelocity, Space.World);

            PublishFeedback();
        }

        private void PublishFeedback()
        {
            var deltaT = Time.fixedDeltaTime;
            var deltaTheta = (float)((vr - vl) * 0.1016 / 0.4826) * deltaT;
            var deltaX = (float)((vl + vr) / 2.0 * Mathf.Cos(deltaTheta)) * deltaT;
            var deltaY = (float)((vl + vr) / 2.0 * Mathf.Sin(deltaTheta)) * deltaT;

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
            Network.Instance.Publish(_feedbackTopic, _feedbackType, data);
        }
    }
}