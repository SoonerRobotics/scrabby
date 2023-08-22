using System;
using Scrabby.State;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace Scrabby
{
    public class PauseMenuController : MonoBehaviour
    {
        public GameObject pauseMenuContainer;
        
        [Header("Buttons")]
        public Button mainMenuButton;
        public Button settingsButton;

        [Header("Other")] 
        public Toggle manualControlToggle;
        
        private void Start()
        {
            mainMenuButton.onClick.AddListener(OnMainMenuPressed);
            // settingsButton.onClick.AddListener(OnSettingsPressed);
            
            manualControlToggle.onValueChanged.AddListener(OnManualControlToggleChanged);
            manualControlToggle.isOn = ScrabbyState.instance.canMoveManually;
        }

        private static void OnMainMenuPressed()
        {
            SceneManager.LoadScene(0);
        }

        private static void OnSettingsPressed()
        {
            
        }
        
        private static void OnManualControlToggleChanged(bool isOn)
        {
            ScrabbyState.instance.canMoveManually = isOn;
        }

        private void TogglePauseMenu()
        {
            pauseMenuContainer.SetActive(!pauseMenuContainer.activeSelf);
        }

        private void Update()
        {
            if (Input.GetKeyUp(KeyCode.Escape))
            {
                TogglePauseMenu();
            }
        }
    }
}
