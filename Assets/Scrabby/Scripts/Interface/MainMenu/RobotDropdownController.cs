using System;
using System.Linq;
using Scrabby.State;
using TMPro;
using UnityEngine;

namespace Scrabby.Interface.MainMenu
{
    public class RobotDropdownController : MonoBehaviour
    {
        private TMP_Dropdown _dropdown;

        private void Start()
        {
            _dropdown = GetComponentInChildren<TMP_Dropdown>();
            _dropdown.ClearOptions();
            _dropdown.AddOptions(ScrabbyState.instance.robots.Select(r => r.name).ToList());

            var controller = FindObjectOfType<RobotSettingsController>();
            if (controller == null) return;
            
            controller.Init();
        }
    }
}