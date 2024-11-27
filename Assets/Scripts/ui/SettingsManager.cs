using UnityEngine;
using UnityEngine.UIElements;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement;

public class SettingsManager : MonoBehaviour {
    public static SettingsManager Instance;
    public static bool manualEnabled;
    public static bool paused = false;
    public static bool showHUD = true;
    public static bool fieldOriented = false;

    void Awake() {
        if (Instance != null) {
            Destroy(gameObject);
        } else {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            // manualEnabled = false;
            // paused = false;
            // showHUD = true;
            // fieldOriented = false;
        }
    }

    void Update() {
        // https://docs.unity3d.com/6000.0/Documentation/ScriptReference/Input.GetKeyDown.html
        if (Input.GetKeyDown("p")) {
            // as long as we're not on the main menu, allow user to pause simulation
            if (SceneManager.GetSceneAt(0).name != "menu") {
                paused = !paused;
            }
        }

        if (paused) {
            Time.timeScale = 0.0f;

            if (SceneManager.sceneCount == 1) {
                SceneManager.LoadScene("pauseMenu", LoadSceneMode.Additive);
            }
        } else {
            Time.timeScale = 1.0f;
            if (SceneManager.sceneCount == 2) {
                SceneManager.UnloadSceneAsync("pauseMenu");
            }
        }
    }
}