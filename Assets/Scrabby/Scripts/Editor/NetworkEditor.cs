using Scrabby.Gameplay;
using Scrabby.Networking;
using Scrabby.Networking.PyScrabby;
using Scrabby.State;
using UnityEditor;
using UnityEngine;

namespace Scrabby.Editor
{
    [CustomEditor(typeof(Networking.Network))]
    public class NetworkEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            var network = (Networking.Network)target;
            var pyscrabby = network.GetPyScrabbyConnection();

            var port = "NONE";
            if (pyscrabby != null)
            {
                port = pyscrabby.Port.ToString();
            }

            EditorGUILayout.LabelField("Port", port);
            ScrabbyState.ShowIncomingMessages = EditorGUILayout.Toggle("Show Incoming Messages", ScrabbyState.ShowIncomingMessages);
            ScrabbyState.ShowOutgoingMessages = EditorGUILayout.Toggle("Show Outgoing Messages", ScrabbyState.ShowOutgoingMessages);

            EditorGUILayout.LabelField("Networks", network.Networks.Count.ToString());
            EditorGUI.indentLevel++;
            if (Application.isPlaying)
            {
                ScrabbyState.Instance.SetNetworkEnabled(NetworkType.PyScrabby, EditorGUILayout.Toggle("PyScrabby", ScrabbyState.Instance.IsNetworkEnabled(NetworkType.PyScrabby)));
                ScrabbyState.Instance.SetNetworkEnabled(NetworkType.Ros, EditorGUILayout.Toggle("ROS", ScrabbyState.Instance.IsNetworkEnabled(NetworkType.Ros)));
                ScrabbyState.Instance.SetNetworkEnabled(NetworkType.Storm, EditorGUILayout.Toggle("Storm", ScrabbyState.Instance.IsNetworkEnabled(NetworkType.Storm)));
            }
            EditorGUI.indentLevel--;
        }
    }
}