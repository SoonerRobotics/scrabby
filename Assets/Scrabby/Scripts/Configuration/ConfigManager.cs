using Scrabby.Utilities;
using UnityEngine;

namespace Scrabby.Configuration
{
    public class ConfigManager : MonoSingleton<ConfigManager>
    {
        public static RobotConfigManager RobotConfig => instance._robotConfig;
        public static GameConfig GameConfig => instance._gameConfig;
        
        private RobotConfigManager _robotConfig;
        private GameConfig _gameConfig;

        protected override void Init()
        {
            EnsureDirectory();
            Debug.Log($"Config Path -> {Application.persistentDataPath}/Config");
            
            _robotConfig = new RobotConfigManager();
            _robotConfig.Init();

            _gameConfig = new GameConfig();
            _gameConfig.Init();
            _gameConfig.Apply();
        }

        private static void EnsureDirectory()
        {
            var configPath = $"{Application.persistentDataPath}/Config";
            if (!System.IO.Directory.Exists(configPath))
            {
                System.IO.Directory.CreateDirectory(configPath);
            }
        }
    }
}