using UnityEngine;

public class SettingsManager : MonoBehaviour {
    public static SettingsManager Instance;
    public static bool manualEnabled;

    void Awake() {
        if (Instance != null) {
            Destroy(gameObject);
        } else {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            manualEnabled = false;
        }
    }
}