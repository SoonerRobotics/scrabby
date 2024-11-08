using UnityEngine;
using UnityEngine.UIElements;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement;

public class MenuUI : MonoBehaviour
{
    private Button _button;
    private Toggle _toggle;
    
    private void OnEnable()
    {
        // The UXML is already instantiated by the UIDocument component
        var uiDocument = GetComponent<UIDocument>();

        _button = uiDocument.rootVisualElement.Q("startButton") as Button;
        _toggle = uiDocument.rootVisualElement.Q("manualToggle") as Toggle;

        _button.RegisterCallback<ClickEvent>(StartScrabby);
        _toggle.RegisterCallback<ClickEvent>(ToggleManualControl);
    }

    private void OnDisable()
    {
        _button.UnregisterCallback<ClickEvent>(StartScrabby);
        _toggle.UnregisterCallback<ClickEvent>(ToggleManualControl);
    }

    private void StartScrabby(ClickEvent evt)
    {
        SceneManager.LoadScene("2025");
    }

    private void ToggleManualControl(ClickEvent evt) {
        SettingsManager.manualEnabled = !SettingsManager.manualEnabled;
    }
}