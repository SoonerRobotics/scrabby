using System.Linq;
using Scrabby.State;
using TMPro;
using UnityEngine;
using UnityEngine.Serialization;

namespace Scrabby.Interface.MainMenu
{
    public class CategoryDropdownController : MonoBehaviour
    {
        private TMP_Dropdown _dropdown;
        
        public GameObject mapDropdown;

        private void Start()
        {
            _dropdown = GetComponentInChildren<TMP_Dropdown>();
            _dropdown.ClearOptions();
            _dropdown.AddOptions(ScrabbyState.Instance.maps.Select(m => m.category).Distinct().ToList());
            
            _dropdown.onValueChanged.AddListener(OnValueChanged);
            OnValueChanged(0);
        }
        
        private void OnValueChanged(int value)
        {
            var category = _dropdown.options[value].text;
            var maps = ScrabbyState.Instance.maps.Where(m => m.category == category).ToList();
            
            mapDropdown.GetComponentInChildren<TMP_Dropdown>().ClearOptions();
            mapDropdown.GetComponentInChildren<TMP_Dropdown>().AddOptions(maps.Select(m => m.name).ToList());
        }
    }
}