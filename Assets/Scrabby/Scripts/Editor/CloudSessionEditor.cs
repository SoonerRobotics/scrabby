using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Scrabby.Editor
{
    public static class CloudSessionEditor
    {
        private const string IsCloudSessionStr = "SCR/Is Cloud Session";

        public static bool IsCloudSession
        {
            get => EditorPrefs.HasKey(IsCloudSessionStr) && EditorPrefs.GetBool(IsCloudSessionStr);
            set => EditorPrefs.SetBool(IsCloudSessionStr, value);
        }

        [MenuItem(IsCloudSessionStr, false, 150)]
        private static void PlayFromFirstSceneCheckMenu() 
        {
            IsCloudSession = !IsCloudSession;
            Menu.SetChecked(IsCloudSessionStr, IsCloudSession);

            ShowNotifyOrLog(IsCloudSession ? "Cloud Session" : "Regular Session");
        }
        
        [MenuItem(IsCloudSessionStr, true)]
        private static bool PlayFromFirstSceneCheckMenuValidate()
        {
            Menu.SetChecked(IsCloudSessionStr, IsCloudSession);
            return true;
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
