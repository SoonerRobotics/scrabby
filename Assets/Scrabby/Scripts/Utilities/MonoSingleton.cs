using UnityEngine;

namespace Scrabby.Utilities
{
    public class MonoSingleton<T> : MonoBehaviour
    {
        public static T Instance;

        private void Awake()
        {
            Instance = (T) (object) this;
            Init();
        }

        private void OnDestroy()
        {
            Instance = default;
        }

        private void OnApplicationQuit()
        {
            Instance = default;
        }
        
        protected virtual void Init()
        {}
    }
}