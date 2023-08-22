using System;
using Scrabby.State;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Scrabby.Interface.MainMenu
{
    public class PlayButton : MonoBehaviour
    {
        public TMP_Dropdown robotDropdown;
        public TMP_Dropdown mapDropdown;

        private Button _button;
        
        private void Start()
        {
            _button = GetComponentInChildren<Button>();
            _button.onClick.AddListener(OnClick);
        }
        
        private void OnClick()
        {
            var robotName = robotDropdown.options[robotDropdown.value].text;
            var mapName = mapDropdown.options[mapDropdown.value].text;
            
            var robot = ScrabbyState.Instance.robots.Find(r => r.name == robotName);
            var map = ScrabbyState.Instance.maps.Find(m => m.name == mapName);
            
            if (robot == null || map == null)
            {
                Debug.LogError($"Robot or map not found: {robotName} {mapName}");
                return;
            }
            
            Debug.Log($"Play {robot} on {map}");
        }
    }
}