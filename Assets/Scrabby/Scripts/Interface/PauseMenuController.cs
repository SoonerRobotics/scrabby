using Scrabby.State;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Network = Scrabby.Networking.Network;

namespace Scrabby.Interface
{
    public class PauseMenuController : MonoBehaviour
    {
        public GameObject pauseMenuContainer;
        
        [Header("Buttons")]
        public Button mainMenuButton;
        public Button restartButton;
        public Button resumeButton;

        [Header("Other")] 
        public Toggle manualControlToggle;
        public Toggle resetNetworkToggle;
        
        private void Start()
        {
            mainMenuButton.onClick.AddListener(OnMainMenuPressed);
            restartButton.onClick.AddListener(OnRestartPressed);
            resumeButton.onClick.AddListener(TogglePauseMenu);
            
            manualControlToggle.onValueChanged.AddListener(OnManualControlToggleChanged);
            manualControlToggle.isOn = ScrabbyState.instance.canMoveManually;
            
            pauseMenuContainer.SetActive(false);
        }

        private void OnDestroy()
        {
            mainMenuButton.onClick.RemoveListener(OnMainMenuPressed);
            restartButton.onClick.RemoveListener(OnRestartPressed);
            resumeButton.onClick.RemoveListener(TogglePauseMenu);
            
            manualControlToggle.onValueChanged.RemoveListener(OnManualControlToggleChanged);
        }

        private void OnMainMenuPressed()
        {
            Network.instance.Close();
            ScrabbyState.instance.movementEnabled = true;
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
