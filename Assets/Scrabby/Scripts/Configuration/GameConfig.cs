using System;
using System.Collections.Generic;
using System.Globalization;
using Newtonsoft.Json;
using UnityEngine;

namespace Scrabby.Configuration
{
    public class GameConfig
    {
        private static string absolutePath => $"{Application.persistentDataPath}/Config/game.json";

        /// <summary>
        /// The game's screen mode
        /// </summary>
        public ScreenMode ScreenMode = ScreenMode.Windowed;

        /// <summary>
        /// Syncs the game's framerate with the monitor's refresh rate
        /// </summary>
        public bool VSync = false;

        /// <summary>
        /// The maximum frame rate the game should be allowed to play at
        /// </summary>
        public int FrameRateCap = 60;
        
        public void Init()
        {
            if (!System.IO.File.Exists(absolutePath))
            {
                Save();
                return;
            }
            
            var fileContent = System.IO.File.ReadAllText(absolutePath);
            var gameConfig = JsonConvert.DeserializeObject<GameConfig>(fileContent);
            LoadFrom(gameConfig);
        }

        public void LoadFrom(GameConfig config)
        {
            ScreenMode = config.ScreenMode;
            VSync = config.VSync;
            FrameRateCap = config.FrameRateCap;
        }
        
        public void Save()
        {
            var fileContent = JsonConvert.SerializeObject(this);
            System.IO.File.WriteAllText(absolutePath, fileContent);
            Debug.Log("Saving Game Configuration");
        }

        public void Apply()
        {
            Screen.fullScreenMode = ScreenMode switch
            {
                ScreenMode.Windowed => FullScreenMode.Windowed,
                ScreenMode.Fullscreen => FullScreenMode.FullScreenWindow,
                ScreenMode.Borderless => FullScreenMode.ExclusiveFullScreen,
                _ => throw new ArgumentOutOfRangeException()
            };
            
            QualitySettings.vSyncCount = VSync ? 1 : 0;
            Application.targetFrameRate = FrameRateCap;
        }
        
        public GameConfig Clone()
        {
            return new GameConfig
            {
                ScreenMode = ScreenMode,
                VSync = VSync,
                FrameRateCap = FrameRateCap
            };
        }

        public bool IsRoughlyEqual(GameConfig config)
        {
            return ScreenMode == config.ScreenMode &&
                   VSync == config.VSync &&
                   FrameRateCap == config.FrameRateCap;
        }
    }

    public enum ScreenMode
    {
        Windowed,
        Fullscreen,
        Borderless
    }
}