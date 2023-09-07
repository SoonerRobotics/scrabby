using Scrabby.Gameplay;
using Scrabby.Networking;
using Scrabby.Networking.PyScrabby;
using Scrabby.State;
using UnityEditor;

namespace Scrabby.Editor
{
    [CustomEditor(typeof(Network))]
    public class NetworkEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
            
            var network = (Network) target;
            var pyscrabby = network.GetPyScrabbyConnection();
            
            EditorGUILayout.LabelField("Port", pyscrabby.GetPort().ToString());
            EditorGUILayout.LabelField("Num Clients", pyscrabby.GetNumClients().ToString());
            EditorGUILayout.LabelField("Message Queue Size", pyscrabby.MessagesInQueue().ToString());
            
            ScrabbyState.ShowIncomingMessages = EditorGUILayout.Toggle("Show Incoming Messages", ScrabbyState.ShowIncomingMessages);
            ScrabbyState.ShowOutgoingMessages = EditorGUILayout.Toggle("Show Outgoing Messages", ScrabbyState.ShowOutgoingMessages);
        }
    }
}