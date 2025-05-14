using System.Net.Sockets;
using System.Threading;
using Unity.Robotics.ROSTCPConnector;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Scrabby
{
    public class ROSHelper : MonoBehaviour
    {
        private ROSConnection _ros;
        private System.Reflection.FieldInfo _hasConnectionErrorField;
        public float frequency = 0.5f;
        private float _nextCheckTime = 0f;
        private bool _hasConnectionError = false;

        void Start()
        {
            _ros = ROSConnection.GetOrCreateInstance();

            // Get the "m_HasConnectionError" field
            var hasConnectionErrorField = typeof(ROSConnection).GetField("m_HasConnectionError", System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.NonPublic);
            if (hasConnectionErrorField != null)
            {
                _hasConnectionErrorField = hasConnectionErrorField;
            }
            else
            {
                Debug.LogError("Failed to get m_HasConnectionError field from ROSConnection.");
            }
        }

        void Update()
        {
            _nextCheckTime += Time.deltaTime;

            if (_nextCheckTime >= frequency)
            {
                if (_ros != null && _hasConnectionErrorField != null)
                {
                    bool hasConnectionError = (bool)_hasConnectionErrorField.GetValue(_ros);
                    if (hasConnectionError && !_hasConnectionError && SceneManager.GetActiveScene().name != "Main Menu")
                    {
                        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
                        Debug.Log("Reloading scene due to ROS connection error.");
                        _hasConnectionError = true;
                    }

                    if (!hasConnectionError && _hasConnectionError)
                    {
                        _hasConnectionError = false;
                    }
                }
                _nextCheckTime = 0f;
            }
        }
    }
}