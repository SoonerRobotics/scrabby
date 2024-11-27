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
    public static GameObject pauseUIDoc;

    void Awake() {
        if (Instance != null) {
            Destroy(gameObject);
        } else {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            manualEnabled = false;
            pauseUIDoc = GameObject.Find("PauseUIDoc");
            DontDestroyOnLoad(pauseUIDoc);
        }
    }

    void Update() {
        // https://docs.unity3d.com/6000.0/Documentation/ScriptReference/Input.GetKeyDown.html
        if (Input.GetKeyDown("p")) {
            paused = !paused;
        }

        if (paused) {
            Time.timeScale = 0.0f;
            // pauseUIDoc.SetActive(true);
            pauseUIDoc.GetComponent<UIDocument>().rootVisualElement.style.opacity = 1.0f;
            // pauseUIDoc.GetComponent<UIDocument>().rootVisualElement.SetEnabled(true);
        } else {
            Time.timeScale = 1.0f;

            // if (pauseUIDoc.GetComponent<UIDocument>().rootVisualElement != null) {
                // pauseUIDoc.GetComponent<UIDocument>().rootVisualElement.ElementAt(0).visible = false;
                // pauseUIDoc.GetComponent<UIDocument>().rootVisualElement.SetEnabled(false);

                pauseUIDoc.GetComponent<UIDocument>().rootVisualElement.style.opacity = 0.0f;
                // pauseUIDoc.SetActive(false);
            // }
        }
    }
}