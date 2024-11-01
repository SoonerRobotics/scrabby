// using UnityEditor;
// using UnityEngine;

// // https://discussions.unity.com/t/is-there-a-way-to-draw-center-of-mass-on-the-screen/50440
// [CustomEditor(typeof(Rigidbody))]
// public class RigidbodyEditor : Editor
// {
// 	void OnSceneGUI()
// 	{
// 		Rigidbody rb = target as Rigidbody;
// 		Handles.color = Color.red;
// 		Handles.SphereHandleCap(1, rb.transform.TransformPoint(rb.centerOfMass), rb.rotation, 1f, EventType.Repaint);
// 	}
// 	public override void OnInspectorGUI()
// 	{
// 		GUI.skin = EditorGUIUtility.GetBuiltinSkin(UnityEditor.EditorSkin.Inspector);
// 		DrawDefaultInspector();
// 	}
// }