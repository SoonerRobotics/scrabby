using System;
using TMPro;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

namespace Scrabby.Interface.Toasts
{
    public class Toast : MonoBehaviour
    {
        public TMP_Text title;
        public TMP_Text message;
        public Image border;
        private Vector2 _targetPosition;

        private int _stage = -1;
        private float _startedAt;

        public ToastType Type { get; private set; }
        public ToastPosition Position { get; private set; }
        public float Duration { get; private set; }

        public void Setup(string toastTitle, string toastMessage, ToastType type, float duration, ToastPosition position)
        {
            var content = transform.Find("Content");
            title = content.Find("Title").GetComponent<TMP_Text>();
            message = content.Find("Message").GetComponent<TMP_Text>();
            border = transform.Find("Border").GetComponent<Image>();

            title.text = toastTitle;
            message.text = toastMessage;
            border.color = ToastManager.GetToastColor(type);

            Type = type;
            Position = position;
            Duration = duration;
        }

        public void Show()
        {
            _stage = 1;
            _startedAt = Time.time;
            LeanTween.scale(gameObject, Vector3.one, 0.3f);
        }

        private void Update()
        {
            switch (_stage)
            {
                case 0: return;
                case 2:
                {
                    if (!(Time.time - _startedAt >= Duration)) return;

                    _stage = 3;
                    _startedAt = Time.time;
                    LeanTween.scale(gameObject, Vector3.zero, 0.5f);
                }
                    break;
                case 1 or 3:
                {
                    var time = Time.time - _startedAt;
                    var progress = time / 0.5f;
                    switch (_stage)
                    {
                        case 1 when progress >= 1:
                            _stage = 2;
                            _startedAt = Time.time;
                            return;
                        case 3 when progress >= 1:
                            ToastManager.Instance.OnToastClosed(this);
                            break;
                    }

                    break;
                }
            }
        }
    }
}