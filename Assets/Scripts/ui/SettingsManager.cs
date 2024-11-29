using UnityEngine;
using UnityEngine.UIElements;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement;

public class SettingsManager : MonoBehaviour {
    public static SettingsManager Instance;
    public static bool manualEnabled;
    public static bool paused;
    public static bool showHUD;
    public static bool fieldOriented;
    public static string cameraView;

    void Awake() {
        if (Instance != null) {
            Destroy(gameObject);
        } else {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            LoadPreferences();
        }
    }

    void Update() {
        // https://docs.unity3d.com/6000.0/Documentation/ScriptReference/Input.GetKeyDown.html
        if (Input.GetKeyDown("p")) {
            // as long as we're not on the main menu, allow user to pause simulation
            if (SceneManager.GetSceneAt(0).name != "menu") {
                paused = !paused;
                SavePreferences();
            }
        }

        if (paused) {
            Time.timeScale = 0.0f;

            if (SceneManager.sceneCount == 1) {
                SceneManager.LoadScene("pauseMenu", LoadSceneMode.Additive);
                UnityEngine.Cursor.lockState = CursorLockMode.None; // allow cursor to move if paused so user can interact with the UI
            }
        } else {
            Time.timeScale = 1.0f;
            if (SceneManager.sceneCount == 2) {
                SceneManager.UnloadSceneAsync("pauseMenu");
            }
        }
    }

    public static void SavePreferences() {
        PlayerPrefs.SetInt("manualEnabled", BoolToInt(manualEnabled));
        PlayerPrefs.SetInt("showHUD", BoolToInt(showHUD));
        PlayerPrefs.SetInt("fieldOriented", BoolToInt(fieldOriented));
        PlayerPrefs.SetString("cameraView", cameraView);
    }

    public static void LoadPreferences() {
        manualEnabled = IntToBool(PlayerPrefs.GetInt("manualEnabled", 0));
        showHUD = IntToBool(PlayerPrefs.GetInt("showHUD", 1));
        fieldOriented = IntToBool(PlayerPrefs.GetInt("fieldOriented", 0));
        cameraView = PlayerPrefs.GetString("cameraView", "fixed");
    }

    public static bool IntToBool(int x) {
        if (x == 0) {
            return false;
        }
        return true;
    }

    public static int BoolToInt(bool x) {
        if (x) {
            return 1;
        }
        return 0;
    }
}