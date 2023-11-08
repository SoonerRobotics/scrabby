using UnityEngine;

namespace Scrabby.Utilities
{
    public class MonoSingleton<T> : MonoBehaviour where T : MonoSingleton<T>
    {
        private static T _instance = null;

        public static T Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = FindObjectOfType(typeof(T)) as T;
                }
                return _instance;
            }
        }

        private void Awake()
        {
            if (_instance != null)
            {
                Destroy(gameObject);
                return;
            }
            
            _instance = this as T;
            if (_instance != null) _instance.Init();
        }

        private void OnDestroy()
        {
            _instance = null;
        }

        private void OnApplicationQuit()
        {
            _instance = null;
        }
        
        protected virtual void Init()
        {}
    }
}