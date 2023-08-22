using UnityEngine;
using UnityEngine.UI;

namespace Scrabby.Interface.MainMenu
{
    public class NavbarButton : MonoBehaviour
    {
        public GameObject targetPage;
        
        private Button _button;

        private void Start()
        {
            _button = GetComponent<Button>();
            _button.onClick.AddListener(OnClick);
        }
        
        private void OnClick()
        {
            Debug.Log($"Clicked {name}");
            foreach (Transform child in targetPage.transform.parent)
            {
                child.gameObject.SetActive(false);
            }
            targetPage.SetActive(true);
        }
    }
}