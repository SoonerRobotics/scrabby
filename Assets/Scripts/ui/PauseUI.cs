using UnityEngine;
using UnityEngine.UIElements;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement;

public class PauseUI : MonoBehaviour
{
    private Toggle manualControlToggle;
    private Toggle fieldOrientedControl;
    private Toggle showHUD;
    private DropdownField cameraDropdown;
    private Button restartButton;
    private Button mainMenuButton;
    
    private void OnEnable()
    {
        // The UXML is already instantiated by the UIDocument component
        var uiDocument = GetComponent<UIDocument>();

        manualControlToggle = uiDocument.rootVisualElement.Q("manualToggle") as Toggle;
        fieldOrientedControl = uiDocument.rootVisualElement.Q("fieldOrientedToggle") as Toggle;
        showHUD = uiDocument.rootVisualElement.Q("showHUDToggle") as Toggle;
        cameraDropdown = uiDocument.rootVisualElement.Q("cameraDropdown") as DropdownField;
        restartButton = uiDocument.rootVisualElement.Q("restartButton") as Button;
        mainMenuButton = uiDocument.rootVisualElement.Q("mainMenuButton") as Button;

        manualControlToggle.value = SettingsManager.manualEnabled;
        fieldOrientedControl.value = SettingsManager.fieldOriented;
        showHUD.value = SettingsManager.showHUD;

        cameraDropdown.choices = new List<string> { "fixed", "mouse", "auto", "bird's eye", "cinematic"};
        cameraDropdown.value = cameraDropdown.choices[0];

        manualControlToggle.RegisterCallback<ClickEvent>(ToggleManualControl);
        fieldOrientedControl.RegisterCallback<ClickEvent>(ToggleFieldOriented);
        showHUD.RegisterCallback<ClickEvent>(ToggleHUD);
        cameraDropdown.RegisterCallback<ClickEvent>(TODO);
        restartButton.RegisterCallback<ClickEvent>(RestartCallback);
        mainMenuButton.RegisterCallback<ClickEvent>(BackToMenu);
    }

    private void OnDisable()
    {
        manualControlToggle.UnregisterCallback<ClickEvent>(ToggleManualControl);
        fieldOrientedControl.UnregisterCallback<ClickEvent>(ToggleFieldOriented);
        showHUD.UnregisterCallback<ClickEvent>(ToggleHUD);
        cameraDropdown.UnregisterCallback<ClickEvent>(TODO);
        mainMenuButton.UnregisterCallback<ClickEvent>(BackToMenu);
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

    private void TODO(ClickEvent evt) {
        //TODO
    }

    private void BackToMenu(ClickEvent evt)
    {
        //FIXME
        SceneManager.UnloadSceneAsync("2025");
        SceneManager.LoadScene("menu");
    }

    private void RestartCallback(ClickEvent evt) {
        //TODO
    }
}