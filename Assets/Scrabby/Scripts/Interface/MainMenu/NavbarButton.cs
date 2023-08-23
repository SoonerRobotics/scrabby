using UnityEngine;
using UnityEngine.UI;

namespace Scrabby.Interface.MainMenu
{
    public class NavbarButton : MonoBehaviour
    {
        public GameObject targetPage;
        public Color activeColor;
        public Color inactiveColor;
        public bool startActive;
        
        private Button _button;

        private void Start()
        {
            _button = GetComponent<Button>();
            _button.onClick.AddListener(OnClick);

            if (startActive)
            {
                GetComponent<Image>().color = activeColor;
            }
        }
        
        private void OnClick()
        {
            foreach (Transform child in targetPage.transform.parent)
            {
                child.gameObject.SetActive(false);
            }
            targetPage.SetActive(true);
            
            foreach (Transform child in gameObject.transform.parent)
            {
                var image = child.GetComponent<Image>();
                if (image == null || child.GetComponent<NavbarButton>() == null) continue;
                
                image.color = inactiveColor;
            }
            GetComponent<Image>().color = activeColor;
        }
    }
}