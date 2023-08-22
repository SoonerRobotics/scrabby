using System;
using System.Collections.Generic;
using System.Globalization;
using Newtonsoft.Json;
using Scrabby.ScriptableObjects;
using Scrabby.Utilities;
using UnityEngine;

namespace Scrabby.Configuration
{
    public class RobotConfigManager
    {
        private const string RobotSettingsPath = "Config/robots.json";

        private Dictionary<string, List<RobotOption>> _robotOptions;

        public void Init()
        {
            var rootPath = Application.persistentDataPath;
            var robotSettingsPath = $"{rootPath}/{RobotSettingsPath}";
            
            if (!System.IO.File.Exists(robotSettingsPath))
            {
                _robotOptions = new Dictionary<string, List<RobotOption>>();
                Save();
                return;
            }
            
            var fileContent = System.IO.File.ReadAllText(robotSettingsPath);
            _robotOptions = JsonConvert.DeserializeObject<Dictionary<string, List<RobotOption>>>(fileContent);
        }

        private bool IsRobotOutdated(Robot robot)
        {
            if (!_robotOptions.ContainsKey(robot.id))
            {
                return true;
            }
            
            var opts = _robotOptions[robot.id];
            if (opts.Count != robot.options.Count)
            {
                return true;
            }
            
            foreach (var option in opts)
            {
                var robotOption = robot.options.Find(o => o.key == option.key);
                if (robotOption == null)
                {
                    return true;
                }

                if (robotOption.type != option.type)
                {
                    return true;
                }
            }
            
            return false;
        }

        public void EnsureRobot(Robot robot)
        {
            if (IsRobotOutdated(robot))
            {
                Debug.LogWarning($"Robot {robot.id} is outdated, recreating configuration");
                _robotOptions.Remove(robot.id);
            }
            if (_robotOptions.ContainsKey(robot.id))
            {
                return;
            }
            
            _robotOptions.Add(robot.id, robot.options);
            Save();
        }

        private void Save()
        {
            var rootPath = Application.persistentDataPath;
            var robotSettingsPath = $"{rootPath}/{RobotSettingsPath}";

            var fileContent = JsonConvert.SerializeObject(_robotOptions);
            System.IO.File.WriteAllText(robotSettingsPath, fileContent);
            Debug.Log("Saving Robot Configuration");
        }

        public void SetKeyValue(string robot, string key, string value)
        {
            if (!_robotOptions.ContainsKey(robot))
            {
                return;
            }
            
            var robotOptions = _robotOptions[robot];
            var option = robotOptions.Find(o => o.key == key);
            if (option == null)
            {
                return;
            }
            
            option.value = value;
            Save();
        }

        public void SetKeyValue(string robot, string key, float value)
        {
            SetKeyValue(robot, key, value.ToString(CultureInfo.CurrentCulture));
        }
        
        public void SetKeyValue(string robot, string key, int value)
        {
            SetKeyValue(robot, key, value.ToString(CultureInfo.CurrentCulture));
        }
        
        public void SetKeyValue(string robot, string key, bool value)
        {
            SetKeyValue(robot, key, value.ToString(CultureInfo.CurrentCulture));
        }
        
        public T GetKeyValue<T>(string robot, string key, T defaultValue = default)
        {
            if (!_robotOptions.ContainsKey(robot))
            {
                return defaultValue;
            }
            
            var robotOptions = _robotOptions[robot];
            var option = robotOptions.Find(o => o.key == key);
            if (option == null)
            {
                return defaultValue;
            }

            try
            {
                return (T) Convert.ChangeType(option.value, typeof(T));
            } catch (Exception)
            {
                Debug.LogError($"Failed to convert {option.value} to {typeof(T)}");
                return defaultValue;
            }
        }
    }
}