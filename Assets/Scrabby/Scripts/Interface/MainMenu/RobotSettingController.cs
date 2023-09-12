using Scrabby.ScriptableObjects;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Scrabby.Interface
{
    public class RobotSettingController : MonoBehaviour
    {
        private RobotOption _option;

        public TMP_Text label;
        public GameObject textOptionContainer;
        public GameObject booleanOptionContainer;
        
        private Toggle _toggle;
        private TMP_InputField _inputField;

        public void Initialize(RobotOption option)
        {
            _option = option;

            if (_option.type == RobotOptionType.Boolean)
            {
                booleanOptionContainer.SetActive(true);
                _toggle = booleanOptionContainer.GetComponentInChildren<Toggle>();
                _toggle.isOn = _option.value == "true";
                _toggle.onValueChanged.AddListener(b => OnValueChanged());
            }
            else
            {
                textOptionContainer.SetActive(true);
                _inputField = textOptionContainer.GetComponentInChildren<TMP_InputField>();
                _inputField.text = _option.value;
                _inputField.onValueChanged.AddListener(s => OnValueChanged());
            }
            
            label.text = _option.key;
        }

        private void OnValueChanged()
        {
            if (_option.type == RobotOptionType.Boolean)
            {
                _option.value = _toggle.isOn ? "true" : "false";
            }
            else
            {
                _option.value = _inputField.text;
            }
        }
    }
}
