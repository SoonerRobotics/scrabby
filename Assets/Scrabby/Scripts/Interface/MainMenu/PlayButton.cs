using System;
using Scrabby.ScriptableObjects;
using Scrabby.State;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
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
            
            var robot = ScrabbyState.instance.robots.Find(r => r.name == robotName);
            var map = ScrabbyState.instance.maps.Find(m => m.name == mapName);
            
            if (robot == null || map == null)
            {
                Debug.LogError($"Robot or map not found: {robotName} {mapName}");
                return;
            }

            Map.Active = map;
            Robot.Active = robot;
            SceneManager.LoadScene(map.sceneIndex);
        }
    }
}