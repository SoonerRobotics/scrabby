using Scrabby.Gameplay;
using Scrabby.Networking;
using Scrabby.Networking.PyScrabby;
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
        }
    }
}