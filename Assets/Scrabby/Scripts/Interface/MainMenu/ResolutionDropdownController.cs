using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;

namespace Scrabby.Interface.MainMenu
{
    public class ResolutionDropdownController : MonoBehaviour
    {
        private TMP_Dropdown _dropdown;

        private void Start()
        {
            _dropdown = GetComponentInChildren<TMP_Dropdown>();
            _dropdown.ClearOptions();
            var options = Screen.resolutions.Select(resolution => resolution.width + "x" + resolution.height).Reverse().ToList();
            _dropdown.AddOptions(options);
            _dropdown.value = options.IndexOf(Screen.width + "x" + Screen.height);
            _dropdown.onValueChanged.AddListener(OnValueChanged);
        }
        
        private void OnValueChanged(int value)
        {
            var resolution = Screen.resolutions[value];
            Screen.SetResolution(resolution.width, resolution.height, Screen.fullScreen);
        }
    }
}
