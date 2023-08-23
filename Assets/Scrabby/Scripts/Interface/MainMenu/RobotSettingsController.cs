using Scrabby.State;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Scrabby.Interface.MainMenu
{
    public class RobotSettingsController : MonoBehaviour
    {
        public TMP_Dropdown robotDropdown;
        
        public GameObject robotSettingPrefab;
        public RectTransform robotSettingsContainer;
        
        private ScrollRect _scrollRect;

        private void Start()
        {
            robotDropdown.onValueChanged.AddListener(OnValueChanged);
        }

        public void Init()
        {
            OnValueChanged(0);
        }

        private void OnValueChanged(int value)
        {
            var robotName = robotDropdown.options[value].text;
            var robot = ScrabbyState.instance.robots.Find(r => r.name == robotName);
            if (robot == null)
            {
                Debug.LogError($"Robot {robotName} not found");
                return;
            }

            var settings = robot.options;
            foreach (Transform child in robotSettingsContainer)
            {
                Destroy(child.gameObject);
            }
            
            foreach (var setting in settings)
            {
                var settingObject = Instantiate(robotSettingPrefab, robotSettingsContainer);
                var controller = settingObject.GetComponent<RobotSettingController>();
                controller.Initialize(setting);
            }
            
            _scrollRect = GetComponent<ScrollRect>();
            _scrollRect.normalizedPosition = new Vector2(0, 1);
            Canvas.ForceUpdateCanvases();
        }
    }
}
