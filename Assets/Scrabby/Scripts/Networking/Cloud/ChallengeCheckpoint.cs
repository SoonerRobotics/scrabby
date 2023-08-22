using System.Collections.Generic;
using Scrabby.Utilities;
using UnityEngine;

namespace Scrabby.Networking
{
    [RequireComponent(typeof(BoxCollider))]
    public class ChallengeCheckpoint : MonoBehaviour
    {
        public CheckpointType type;
        public string message;
        public List<Utilities.StringKvp> data;
        
        public bool hasActivated = false;
        public bool canMultiActivate = false;
        public List<string> triggeredBy = new List<string>();

        private async void OnTriggerEnter(Collider other)
        {
            if (hasActivated && !canMultiActivate)
            {
                return;
            }
            
            if (triggeredBy.Count > 0 && !triggeredBy.Contains(other.name))
            {
                return;
            }

            if (type == CheckpointType.End)
            {
                await Cloud.instance.SetRunStatus(RunStatus.Finished);
                SceneHelper.Quit();
                return;
            }
            
            await Cloud.instance.Log(LogLevel.Info, message);
        }
    }

    public enum CheckpointType
    {
        Log,
        End
    }
}