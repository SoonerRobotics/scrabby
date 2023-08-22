using System;
using UnityEngine;
using UnityEngine.UI;

namespace Scrabby.Interface.MainMenu
{
    public class QuitButton : MonoBehaviour
    {
        private Button _button;

        private void Start()
        {
            _button = GetComponentInChildren<Button>();
            _button.onClick.AddListener(OnClick);
        }

        private static void OnClick()
        {
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        }
    }
}