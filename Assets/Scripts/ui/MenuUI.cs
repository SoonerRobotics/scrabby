using UnityEngine;
using UnityEngine.UIElements;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement;

public class MenuUI : MonoBehaviour //TODO should PauseUI be like a child of this or something? I don't like duplicating code, can we like stack/merge UI docs and scripts?
{
    private Toggle manualControlToggle;
    private Toggle fieldOrientedControl;
    private Toggle showHUD;
    private DropdownField cameraDropdown;

    private Button startScrabbyButton;

     void Awake() {
        SetupCallbacks();
    }

    void OnLoad() {
        SetupCallbacks();
    }
    
    private void SetupCallbacks()
    {
        // The UXML is already instantiated by the UIDocument component
        var uiDocument = GetComponent<UIDocument>();

        SettingsManager.LoadPreferences();

        manualControlToggle = uiDocument.rootVisualElement.Q("manualToggle") as Toggle;
        fieldOrientedControl = uiDocument.rootVisualElement.Q("fieldOrientedToggle") as Toggle;
        showHUD = uiDocument.rootVisualElement.Q("showHUDToggle") as Toggle;
        cameraDropdown = uiDocument.rootVisualElement.Q("cameraDropdown") as DropdownField;
        
        startScrabbyButton = uiDocument.rootVisualElement.Q("startButton") as Button;

        cameraDropdown.choices = new List<string> { "fixed", "mouse", "auto", "bird's eye", "cinematic"};

        manualControlToggle.value = SettingsManager.IntToBool(PlayerPrefs.GetInt("manualEnabled", 0));
        fieldOrientedControl.value = SettingsManager.IntToBool(PlayerPrefs.GetInt("fieldOriented", 0));
        showHUD.value = SettingsManager.IntToBool(PlayerPrefs.GetInt("showHUD", 1));
        cameraDropdown.value = PlayerPrefs.GetString("cameraDropdown", "fixed");

        manualControlToggle.RegisterCallback<ClickEvent>(ToggleManualControl);
        fieldOrientedControl.RegisterCallback<ClickEvent>(ToggleFieldOriented);
        showHUD.RegisterCallback<ClickEvent>(ToggleHUD);
        cameraDropdown.RegisterCallback<ClickEvent>(SwitchCamera);
        startScrabbyButton.RegisterCallback<ClickEvent>(StartScrabby);
    }

    private void OnDisable()
    {
        manualControlToggle.UnregisterCallback<ClickEvent>(ToggleManualControl);
        fieldOrientedControl.UnregisterCallback<ClickEvent>(ToggleFieldOriented);
        showHUD.UnregisterCallback<ClickEvent>(ToggleHUD);
        cameraDropdown.UnregisterCallback<ClickEvent>(SwitchCamera);
        startScrabbyButton.UnregisterCallback<ClickEvent>(StartScrabby);
    }

    private void ToggleManualControl(ClickEvent evt) {
        SettingsManager.manualEnabled = !SettingsManager.manualEnabled;
    }

    private void ToggleFieldOriented(ClickEvent evt)
    {
        SettingsManager.fieldOriented = !SettingsManager.fieldOriented;
    }

    private void ToggleHUD(ClickEvent evt)
    {
        SettingsManager.showHUD = !SettingsManager.showHUD;
    }

    private void SwitchCamera(ClickEvent evt) {
        //TODO
    }

    private void StartScrabby(ClickEvent evt) {
        SettingsManager.SavePreferences();
        SceneManager.LoadScene("2025", LoadSceneMode.Single);
    }
}