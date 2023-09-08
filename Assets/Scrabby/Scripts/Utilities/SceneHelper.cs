using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Scrabby.Utilities
{
    public static class SceneHelper
    {
        public static void Switch(int index)
        {
            SceneManager.LoadScene(index);
        }

        public static void Quit()
        {
#if UNITY_EDITOR
            EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        }
    }
}