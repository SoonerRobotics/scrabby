using System;
using TMPro;
using UnityEngine;

namespace Scrabby.Interface.MainMenu
{
    public class ScreenModeDropdownController : MonoBehaviour
    {
        private TMP_Dropdown _dropdown;

        private void Start()
        {
            _dropdown = GetComponentInChildren<TMP_Dropdown>();
            _dropdown.onValueChanged.AddListener(OnValueChanged);
            
            _dropdown.value = Screen.fullScreenMode switch
            {
                FullScreenMode.FullScreenWindow => 0,
                FullScreenMode.ExclusiveFullScreen => 1,
                FullScreenMode.Windowed => 2,
                _ => throw new ArgumentOutOfRangeException()
            };
        }
        
        private static void OnValueChanged(int value)
        {
            Screen.fullScreenMode = value switch
            {
                0 => FullScreenMode.FullScreenWindow,
                1 => FullScreenMode.ExclusiveFullScreen,
                2 => FullScreenMode.Windowed,
                _ => throw new ArgumentOutOfRangeException(nameof(value), value, null)
            };
        }
    }
}
