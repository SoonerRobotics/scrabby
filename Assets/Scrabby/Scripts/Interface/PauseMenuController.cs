using Scrabby.State;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Serialization;
using UnityEngine.UI;

namespace Scrabby.Interface
{
    public class PauseMenuController : MonoBehaviour
    {
        public GameObject pauseMenuContainer;
        
        [Header("Buttons")]
        public Button quitButton;
        public Button restartButton;
        public Button resumeButton;

        [Header("Other")] 
        public Toggle manualControlToggle;
        public Toggle resetNetworkToggle;
        
        private void Start()
        {
            quitButton.onClick.AddListener(OnMainMenuPressed);
            restartButton.onClick.AddListener(OnRestartPressed);
            resumeButton.onClick.AddListener(TogglePauseMenu);
            
            manualControlToggle.onValueChanged.AddListener(OnManualControlToggleChanged);
            manualControlToggle.isOn = ScrabbyState.Instance.canMoveManually;
            
            pauseMenuContainer.SetActive(false);
        }

        private void OnDestroy()
        {
            quitButton.onClick.RemoveListener(OnMainMenuPressed);
            restartButton.onClick.RemoveListener(OnRestartPressed);
            resumeButton.onClick.RemoveListener(TogglePauseMenu);
            
            manualControlToggle.onValueChanged.RemoveListener(OnManualControlToggleChanged);
        }

        private void OnMainMenuPressed()
        {
            var originalResetNetworkValue = resetNetworkToggle.isOn;
            ScrabbyState.Instance.movementEnabled = true;
            SceneManager.LoadScene(0);
            gameObject.SetActive(false);
        }

        private static void OnRestartPressed()
        {
            ScrabbyState.Instance.movementEnabled = true;
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        }
        
        private static void OnManualControlToggleChanged(bool isOn)
        {
            ScrabbyState.Instance.canMoveManually = isOn;
        }

        public void TogglePauseMenu()
        {
            pauseMenuContainer.SetActive(!pauseMenuContainer.activeSelf);
            ScrabbyState.Instance.movementEnabled = !pauseMenuContainer.activeSelf;
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
