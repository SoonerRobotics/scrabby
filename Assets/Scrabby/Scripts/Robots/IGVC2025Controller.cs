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

        // physics robot control stuff
        private const float G = 9.8f; // 9.8 m/s^2
        private const float maxVelocity = 2.279904f;
        private const float maxAngularVelocity = 45f; // deg/sec?

        private const float coeff_friction = 0.2f; // typical friction value???
        private const float angular_coeff_friction = 0.2f;

        private const float forwardFrictionForce = G * robotMass * coeff_friction; // newtons
        private const float sideweaysFrictionForce = G * robotMass * coeff_friction; //???
        private const float angularFrictionForce = G * robotMass * angular_coeff_friction; // yes this makes no sense deal with it
        private const float robotMass = 100f; // kilos

        private const float maxLinearForce = 500.0f; // newtons???
        private const float maxAngularForce = 500.0f;

        private float forwardVelocity = 0.0f;
        private float sidewaysVelocity = 0.0f;
        private float angularVelocity = 0.0f;


        private void Start()
        {
            RosConnector.OnNetworkInstruction.AddListener(OnNetworkInstruction);

            _inputForwardField = robot.GetOption("topics.input.forward", "forward_velocity");
            _inputAngularField = robot.GetOption("topics.input.angular", "angular_velocity");
            _inputSidewaysField = robot.GetOption("topics.input.sideways", "sideways_velocity");

            _feedbackTopic = robot.GetOption("topics.feedback", "/autonav/MotorFeedback");
            _feedbackType = robot.GetOption("topics.feedback.type", "autonav_msgs/MotorFeedback");
            _deltaXField = robot.GetOption("topics.feedback.deltax", "delta_x");
            _deltaYField = robot.GetOption("topics.feedback.deltay", "delta_y");
            _deltaThetaField = robot.GetOption("topics.feedback.deltatheta", "delta_theta");

            forwardVelocity = 0.0f;
            sidewaysVelocity = 0.0f;
            angularVelocity = 0.0f;
            
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
            var sideways = instruction.GetData<float>(_inputSidewaysField);
            Debug.Log($"Forward: {forward}, Angular: {angular}, Sideways: {sideways}");
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

            float forwardForce = 0.0f;
            float sidewaysForce = 0.0f;
            float angularForce = 0.0f;

            float forwardAccel = 0.0f;
            float sidewaysAccel = 0.0f;
            float angularAccel = 0.0f;

            if (ScrabbyState.Instance.canMoveManually)
            {
                var horiz = Input.GetAxis("Vertical");
                var vertical = Input.GetAxis("Horizontal");
                var turn = Input.GetAxis("Steer");
                
                forwardForce = horiz * 5;
                sidewaysForce = vertical * 5;
                angularForce = turn * 3;

                // angularForce = 5; // newtons? torque? newton meters?
                // linearForce = 5; //Newtons
            }
            else
            {
                forwardForce = Mathf.Clamp((forwardControl - forwardVelocity), -maxLinearForce, maxLinearForce);
                sidewaysForce = Mathf.Clamp((sidewaysControl - sidewaysVelocity), -maxLinearForce, maxLinearForce);
                angularForce = Mathf.Clamp((angularControl - angularVelocity), -maxAngularForce, maxAngularForce);
            }

            forwardForce -= Mathf.Clamp(forwardFrictionForce, 0, forwardForce); // this is only static friction huh?
            sidewaysForce -= Mathf.Clamp(sideweaysFrictionForce, 0, sideweaysFrictionForce); // same complaint
            angularForce -= Mathf.Clamp(angularFrictionForce, 0, angularFrictionForce); // same complaint

            forwardAccel = forwardForce / robotMass; // newton's 1st law
            sidewaysAccel = sidewaysForce / robotMass;
            angularAccel = angularForce / robotMass; // no this is not correct because torque and stuff, but deal with it

            forwardVelocity += sidewaysAccel * Time.fixedDeltaTime;
            sidewaysVelocity += sidewaysAccel * Time.fixedDeltaTime;
            angularVelocity += angularAccel * Time.fixedDeltaTime;

            forwardVelocity = Mathf.Clamp(forwardVelocity, -maxVelocity, maxVelocity);
            sidewaysVelocity = Mathf.Clamp(sidewaysVelocity, -maxVelocity, maxVelocity);
            angularVelocity = Mathf.Clamp(angularVelocity, -maxAngularVelocity, maxAngularVelocity);

            LinearVelocity = new Vector3(forwardVelocity, 0, sidewaysVelocity);
            AngularVelocity = new Vector3(0, angularVelocity, 0);

            transform.Translate(LinearVelocity * Time.fixedDeltaTime);
            transform.Rotate(AngularVelocity * Time.fixedDeltaTime);

            PublishFeedback();
        }

        private void PublishFeedback()
        {
            //TODO FIXME this is probably not correct
            var deltaT = Time.fixedDeltaTime;
            var deltaTheta = AngularVelocity.y * Time.fixedDeltaTime;
            var deltaX = LinearVelocity.x * Time.fixedDeltaTime;
            var deltaY = LinearVelocity.z * Time.fixedDeltaTime;

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