using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Scrabby
{
    public class FPSCounter : MonoBehaviour
    {
        public TMP_Text fpsText;
        public bool show = false;

        private void Start()
        {
            // Application.targetFrameRate = 75;
            UpdateVisibility();
        }

        private void UpdateVisibility()
        {
            fpsText.enabled = show;
            fpsText.transform.parent.GetComponent<Image>().enabled = show;
        }

        private void Update()
        {
            fpsText.text = (1f / Time.unscaledDeltaTime).ToString("F0") + " FPS";
            
            if (Input.GetKeyDown(KeyCode.P))
            {
                show = !show;
                UpdateVisibility();
            }
        }
    }
}
