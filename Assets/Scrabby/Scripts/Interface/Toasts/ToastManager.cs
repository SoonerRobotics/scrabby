using System.Collections.Generic;
using Scrabby.Utilities;
using UnityEngine;

namespace Scrabby.Interface.Toasts
{
    public class ToastManager : MonoSingleton<ToastManager>
    {
        public int maxToasts = 5;
        public Toast toastPrefab;
        
        public Transform topLeftContainer;
        public Transform topRightContainer;
        public Transform bottomLeftContainer;
        public Transform bottomRightContainer;

        private readonly Dictionary<ToastPosition, List<Toast>> _liveToasts = new();
        private readonly Dictionary<ToastPosition, List<Toast>> _waitingToasts = new();

        protected override void Init()
        {
            foreach (ToastPosition position in System.Enum.GetValues(typeof(ToastPosition)))
            {
                _liveToasts[position] = new List<Toast>();
                _waitingToasts[position] = new List<Toast>();
            }
            
            DontDestroyOnLoad(this);
        }

        public void Show(string title, string message, ToastType type = ToastType.Info, float duration = 2f, ToastPosition position = ToastPosition.TopRight)
        {
            var newToast = CreateToast(title, message, type, duration, position);
            if (_liveToasts[position].Count >= maxToasts)
            {
                _waitingToasts[position].Add(newToast);
                return;
            }
            
            _liveToasts[position].Add(newToast);
            newToast.Show();
        }
        
        private Toast CreateToast(string title, string message, ToastType type, float duration, ToastPosition position)
        {
            var container = position switch
            {
                ToastPosition.TopLeft => topLeftContainer,
                ToastPosition.TopRight => topRightContainer,
                ToastPosition.BottomLeft => bottomLeftContainer,
                ToastPosition.BottomRight => bottomRightContainer,
                _ => topRightContainer
            };
            var newToast = Instantiate(toastPrefab, container);
            newToast.Setup(title, message, type, duration, position);
            return newToast;
        }

        public void OnToastClosed(Toast toast)
        {
            Destroy(toast.gameObject);
            _liveToasts[toast.Position].Remove(toast);
            if (_waitingToasts[toast.Position].Count <= 0) return;
            
            var waitingToast = _waitingToasts[toast.Position][0];
            _waitingToasts[toast.Position].RemoveAt(0);
            _liveToasts[toast.Position].Add(waitingToast);
            waitingToast.Show();
        }

        public static Color GetToastColor(ToastType type)
        {
            return type switch
            {
                ToastType.Info => Color.blue,
                ToastType.Warning => Color.yellow,
                ToastType.Error => Color.red,
                _ => Color.white
            };
        }
    }
}