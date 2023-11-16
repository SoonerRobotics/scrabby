using System;
using TMPro;
using UnityEngine;

namespace Scrabby.Interface
{
    public class RunTimer : MonoBehaviour
    {
        public static RunTimer Instance;
        public TMP_Text text;

        private float _startTime;
        
        public bool IsStarted { get; private set; }

        private void Start()
        {
            Instance = this;
        }

        public void Begin()
        {
            IsStarted = true;
            _startTime = Time.time;
        }

        public void Stop()
        {
            IsStarted = false;
        }

        private void Update()
        {
            if (!IsStarted)
            {
                text.text = "00:00:00";
                return;
            }
            
            var time = TimeSpan.FromSeconds(Time.time - _startTime);
            text.text = $"{time.Minutes:00}:{time.Seconds:00}:{time.Milliseconds:00}";
        }
    }
}