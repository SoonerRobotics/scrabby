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
    public static float positionOffset;
    public static float initialHeading;
    public static string cameraView;
    
    public static bool needToSetPosition = false;
    public static List<string> cameraViewModes = new List<string> { "fixed", "orbit", "auto", "bird's eye", "cinematic" };
    private static int cameraViewIndex = 0;

    void Awake() {
        if (Instance != null) {
            Destroy(gameObject);
        } else {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            LoadPreferences();
            needToSetPosition = false;
        }
    }

    void Update() {
        // https://docs.unity3d.com/6000.0/Documentation/ScriptReference/Input.GetKeyDown.html
        if (Input.GetKeyDown("p") || Input.GetKeyDown("escape")) {
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

        cameraViewIndex = cameraViewModes.FindIndex(x => x.Equals(cameraView));

        if (Input.GetKeyDown("c") || Input.GetKeyDown("right ctrl") || Input.GetKeyDown("left ctrl")) {
            cameraViewIndex += 1;
            if (cameraViewIndex > cameraViewModes.Count-1) {
                cameraViewIndex = 0;
            }

            cameraView = cameraViewModes[cameraViewIndex];
        }
    }

    public static void SavePreferences() {
        PlayerPrefs.SetInt("manualEnabled", BoolToInt(manualEnabled));
        PlayerPrefs.SetInt("showHUD", BoolToInt(showHUD));
        PlayerPrefs.SetInt("fieldOriented", BoolToInt(fieldOriented));
        PlayerPrefs.SetString("cameraView", cameraView);
        PlayerPrefs.SetFloat("positionOffset", positionOffset);
        PlayerPrefs.SetFloat("initialHeading", initialHeading);
    }

    public static void LoadPreferences() {
        manualEnabled = IntToBool(PlayerPrefs.GetInt("manualEnabled", 0));
        showHUD = IntToBool(PlayerPrefs.GetInt("showHUD", 1));
        fieldOriented = IntToBool(PlayerPrefs.GetInt("fieldOriented", 0));
        positionOffset = PlayerPrefs.GetFloat("positionOffset", 0.0f);
        initialHeading = PlayerPrefs.GetFloat("initialHeading", 0.0f);
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