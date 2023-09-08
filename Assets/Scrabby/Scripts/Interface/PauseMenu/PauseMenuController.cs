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
        public Button restartButton;

        [Header("Other")] 
        public Toggle manualControlToggle;
        
        private void Start()
        {
            mainMenuButton.onClick.AddListener(OnMainMenuPressed);
            restartButton.onClick.AddListener(OnRestartPressed);
            
            manualControlToggle.onValueChanged.AddListener(OnManualControlToggleChanged);
            manualControlToggle.isOn = ScrabbyState.instance.canMoveManually;
        }

        private void OnMainMenuPressed()
        {
            SceneManager.LoadScene(0);
            gameObject.SetActive(false);
        }

        private static void OnRestartPressed()
        {
            ScrabbyState.instance.movementEnabled = true;
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        }
        
        private static void OnManualControlToggleChanged(bool isOn)
        {
            ScrabbyState.instance.canMoveManually = isOn;
        }

        public void TogglePauseMenu()
        {
            pauseMenuContainer.SetActive(!pauseMenuContainer.activeSelf);
            ScrabbyState.instance.movementEnabled = !pauseMenuContainer.activeSelf;
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
