using Newtonsoft.Json.Linq;
using Scrabby.Interface;
using Scrabby.Networking;
using Scrabby.ScriptableObjects;
using Scrabby.State;
using UnityEngine;

namespace Scrabby.Robots
{
    public class Igvc2025Controller : BaseRobot
    {
        public Robot robot;

        public float speedMod = 2f;
        public float wheelRadius = 0.1016f;
        public float distanceBetweenWheels = 0.508f;
        public float axleLength = 1f; // ???
        public float minSpeed = 0.1f;
        public float drag = 0.85f;

        public float vf, vs, vr; // ???

        public float forward_, sideways_, rotate_;

        public float forwardControl;
        public float angularControl;
        public float sidewaysControl;

        private Vector3 LinearVelocity { get; set; }
        private Vector3 AngularVelocity { get; set; }

        private string _inputForwardField;
        private string _inputAngularField;
        private string _inputSidewaysField;

        private string _feedbackTopic;
        private string _feedbackType;
        private string _deltaXField;
        private string _deltaYField;
        private string _deltaThetaField;

        private void Start()
        {
            RosConnector.OnNetworkInstruction.AddListener(OnNetworkInstruction);

            _inputForwardField = robot.GetOption("topics.input.forward", "forward_velocity");
            _inputSidewaysField = robot.GetOption("topics.input.sideways", "sideways_velocity");
            _inputAngularField = robot.GetOption("topics.input.angular", "angular_velocity");

            _feedbackTopic = robot.GetOption("topics.feedback", "/autonav/MotorFeedback");
            _feedbackType = robot.GetOption("topics.feedback.type", "autonav_msgs/MotorFeedback");
            _deltaXField = robot.GetOption("topics.feedback.deltax", "delta_x");
            _deltaYField = robot.GetOption("topics.feedback.deltay", "delta_y");
            _deltaThetaField = robot.GetOption("topics.feedback.deltatheta", "delta_theta");
            
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
            var sideways = instruction.GetData<float>(_inputSidewaysField);
            var angular = instruction.GetData<float>(_inputAngularField);
            Debug.Log($"Forward: {forward}, Angular: {angular}, Siways: {sideways}");
            forwardControl = forward;
            angularControl = angular;
            sidewaysControl = sideways;
            
            if ((forward > 0 || angular > 0 || sideways > 0) && !RunTimer.Instance.IsStarted)
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
            
            if (ScrabbyState.Instance.canMoveManually)
            {
                var horiz = Mathf.Pow(Input.GetAxis("Vertical"), 3);
                var vertical = Input.GetAxis("Horizontal");
                var side = Input.GetAxis("Debug Horizontal");
                
                forward_ = vertical * speedMod / wheelRadius;
                sideways_ = side * speedMod / wheelRadius;
                rotate_ = horiz * speedMod / wheelRadius;
                // left = (horiz * vertical + horiz) * speedMod / wheelRadius;
            }
            else
            {
                //TODO
                // left = (forwardControl / wheelRadius) - (distanceBetweenWheels * (angularControl / wheelRadius));
                // right = (forwardControl / wheelRadius) + (distanceBetweenWheels * (angularControl / wheelRadius));
            }

            var psiDot = wheelRadius * (.5) / axleLength;

            if (Mathf.Abs(drag * forward_) < minSpeed)
            {
                forward_ = 0;
            }

            if (Mathf.Abs(drag * sideways_) < minSpeed)
            {
                sideways_ = 0;
            }

            if (Mathf.Abs(drag * rotate_) < minSpeed)
            {
                rotate_ = 0;
            }

            vf = drag * forward_ + (1.0f - drag) * vf;
            vs = drag * sideways_ + (1.0f - drag) * vs;
            vr = drag * rotate_ + (1.0f - drag) * vr;

            // var dotX = wheelRadius * (vf) / 2.0f * Mathf.Sin(psi);
            // var dotY = wheelRadius * (vs) / 2.0f * Mathf.Cos(psi);
            // var dotZ = wheelRadius * (vs) / 2.0f * Mathf.Cos(psi);

            // var dotX = wheelRadius * 

            var newLinearVel = new Vector3(vf, 0, vs);

            LinearVelocity = newLinearVel;
            // AngularVelocity = new Vector3(0, psiDot, 0);
            AngularVelocity = new Vector3(0, vr, 0);
            
            // transform.Translate(LinearVelocity * Time.fixedDeltaTime, Space.World);
            // transform.Rotate(Time.fixedDeltaTime * Mathf.Rad2Deg * AngularVelocity, Space.World);

            // PublishFeedback();
        }

        private void PublishFeedback()
        {
            var deltaT = Time.fixedDeltaTime;
            var deltaTheta = (float)((.5) * wheelRadius / axleLength) * deltaT;
            var deltaX = wheelRadius * (1) / 2.0f * Mathf.Cos(deltaTheta) * deltaT;
            // var deltaY = wheelRadius * (vl + vr) / 2.0f * Mathf.Sin(deltaTheta) * deltaT;
            var deltaY = wheelRadius * (1) / 2.0f * Mathf.Sin(deltaTheta) * deltaT;

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