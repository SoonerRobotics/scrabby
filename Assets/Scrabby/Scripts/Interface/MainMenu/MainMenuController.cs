using System;
using System.Linq;
using Scrabby.State;
using Scrabby.Utilities;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Scrabby.Interface
{
    public class MainMenuController : MonoSingleton<MainMenuController>
    {

        
        public Button quitButton;

        [Header("Settings")]
        public TMP_Dropdown resolutionDropdown;
        public TMP_Dropdown screenModeDropdown;
        public TMP_Dropdown robotOptionDropdown;
        
        private void Start()
        {   
            resolutionDropdown.ClearOptions();
            resolutionDropdown.AddOptions(Screen.resolutions.Select(r => r.width + "x" + r.height).Reverse().ToList());
            resolutionDropdown.value = resolutionDropdown.options.FindIndex(o => o.text == Screen.currentResolution.width + "x" + Screen.currentResolution.height);
            resolutionDropdown.onValueChanged.AddListener(OnResolutionSelected);
            
            screenModeDropdown.ClearOptions();
            screenModeDropdown.AddOptions(Enum.GetNames(typeof(FullScreenMode)).ToList());
            screenModeDropdown.value = screenModeDropdown.options.FindIndex(o => o.text == Screen.fullScreenMode.ToString());
            screenModeDropdown.onValueChanged.AddListener(OnScreenModeSelected);

            robotOptionDropdown.ClearOptions();
            robotOptionDropdown.AddOptions(ScrabbyState.instance.robots.Select(r => r.name).ToList());
            //robotOptionDropdown.onValueChanged.AddListener();
            
            
            quitButton.onClick.AddListener(OnQuit);
        }

        private void OnDestroy()
        {
            resolutionDropdown.onValueChanged.RemoveListener(OnResolutionSelected);
            screenModeDropdown.onValueChanged.RemoveListener(OnScreenModeSelected);
            
            quitButton.onClick.RemoveListener(OnQuit);
        }

        

        private static void OnQuit()
        {
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        }
        
        private void OnResolutionSelected(int index)
        {
            var resolution = Screen.resolutions[index];
            var screenMode = (FullScreenMode) Enum.Parse(typeof(FullScreenMode), screenModeDropdown.options[index].text);
            Screen.SetResolution(resolution.width, resolution.height, screenMode);
        }
        
        private void OnScreenModeSelected(int index)
        {
            var screenMode = (FullScreenMode) Enum.Parse(typeof(FullScreenMode), screenModeDropdown.options[index].text);
            Screen.SetResolution(Screen.currentResolution.width, Screen.currentResolution.height, screenMode);
        }

    }
}
