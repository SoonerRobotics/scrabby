using Scrabby.Gameplay;
using UnityEditor;

namespace Scrabby.Editor
{
    [CustomEditor(typeof(RoadRenderer))]
    [CanEditMultipleObjects]
    public class RoadRendererEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
            
            var roadRenderer = (RoadRenderer) target;
            EditorGUILayout.FloatField("Spline Length", roadRenderer.GetSpineLength());
        }
    }
}