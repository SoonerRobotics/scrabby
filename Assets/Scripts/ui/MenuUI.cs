using UnityEngine;
using UnityEngine.UIElements;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement;

public class MenuUI : MonoBehaviour
{
    private Button _button;
    
    private void OnEnable()
    {
        // The UXML is already instantiated by the UIDocument component
        var uiDocument = GetComponent<UIDocument>();

        _button = uiDocument.rootVisualElement.Q("startButton") as Button;

        _button.RegisterCallback<ClickEvent>(StartScrabby);
    }

    private void OnDisable()
    {
        _button.UnregisterCallback<ClickEvent>(StartScrabby);
    }

    private void StartScrabby(ClickEvent evt)
    {
        SceneManager.LoadScene("2025");
    }
}