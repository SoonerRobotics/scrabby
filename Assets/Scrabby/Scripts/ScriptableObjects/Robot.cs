using System;
using System.Collections.Generic;
using Scrabby.Configuration;
using UnityEngine;
using UnityEngine.Serialization;

namespace Scrabby.ScriptableObjects
{
    [CreateAssetMenu(fileName = "Robot", menuName = "SCR/Robot")]
    [Serializable]
    public class Robot : ScriptableObject
    {
        public static Robot Active;

        public string id;
        public new string name;
        public string year;
        public GameObject prefab;
        public List<RobotOption> options;

        public T GetOption<T>(string key)
        {
            ConfigManager.RobotConfig.EnsureRobot(Active);
            return ConfigManager.RobotConfig.GetKeyValue<T>(id, key);
        }

        public T GetOption<T>(string key, T defaultValue)
        {
            ConfigManager.RobotConfig.EnsureRobot(Active);
            return ConfigManager.RobotConfig.GetKeyValue<T>(id, key, defaultValue);
        }
    }
    
    public enum RobotOptionType
    {
        String,
        Integer,
        Float,
        Boolean
    }

    [Serializable]
    public class RobotOption
    {
        public string key;
        public RobotOptionType type;
        public string value;
        
        public int GetInt()
        {
            return int.Parse(value);
        }

        public int GetInt(int defaultValue)
        {
            return int.TryParse(value, out var result) ? result : defaultValue;
        }
        
        public float GetFloat()
        {
            return float.Parse(value);
        }
        
        public float GetFloat(float defaultValue)
        {
            return float.TryParse(value, out var result) ? result : defaultValue;
        }
        
        public bool GetBool()
        {
            return bool.Parse(value);
        }
        
        public bool GetBool(bool defaultValue)
        {
            return bool.TryParse(value, out var result) ? result : defaultValue;
        }
        
        public string GetString()
        {
            return value;
        }
        
        public string GetString(string defaultValue)
        {
            return string.IsNullOrEmpty(value) ? defaultValue : value;
        }
    }
}
