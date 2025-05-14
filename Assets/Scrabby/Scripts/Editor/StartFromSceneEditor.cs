using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Scrabby.Editor
{
    public static class PlayFromTheFirstScene
    {
        private const string PlayFromFirstMenuStr = "SCR/Start from Scene 0";

        private static bool PlayFromFirstScene
        {
            get => EditorPrefs.HasKey(PlayFromFirstMenuStr) && EditorPrefs.GetBool(PlayFromFirstMenuStr);
            set => EditorPrefs.SetBool(PlayFromFirstMenuStr, value);
        }

        [MenuItem(PlayFromFirstMenuStr, false, 150)]
        private static void PlayFromFirstSceneCheckMenu() 
        {
            PlayFromFirstScene = !PlayFromFirstScene;
            Menu.SetChecked(PlayFromFirstMenuStr, PlayFromFirstScene);

            ShowNotifyOrLog(PlayFromFirstScene ? "Play from scene 0" : "Play from current scene");
        }
        
        [MenuItem(PlayFromFirstMenuStr, true)]
        private static bool PlayFromFirstSceneCheckMenuValidate()
        {
            Menu.SetChecked(PlayFromFirstMenuStr, PlayFromFirstScene);
            return true;
        }
        
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void LoadFirstSceneAtGameBegins()
        {
            if (!PlayFromFirstScene)
            {
                return;
            }

            if(EditorBuildSettings.scenes.Length  == 0)
            {
                Debug.LogWarning("The scene build list is empty. Can't play from first scene.");
                return;
            }

            foreach (var go in Object.FindObjectsByType<GameObject>(FindObjectsSortMode.None))
            {
                go.SetActive(false);
            }

            SceneManager.LoadScene(0);
        }

        private static void ShowNotifyOrLog(string msg)
        {
            if (Resources.FindObjectsOfTypeAll<SceneView>().Length > 0)
            {
                EditorWindow.GetWindow<SceneView>().ShowNotification(new GUIContent(msg));
            }
            else
            {
                Debug.Log(msg);
            }
        }
    }
}
