using System.Collections.Generic;
using Scrabby.ScriptableObjects;
using Scrabby.Utilities;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Scrabby.Gameplay
{
    public class WaypointVisualizer : MonoSingleton<WaypointVisualizer>
    {
        public GameObject waypointPrefab;

        private List<GameObject> waypointObjects = new();
        
        private void Start()
        {            
            SceneManager.sceneLoaded += OnSceneLoaded;
            SceneManager.sceneUnloaded += OnSceneUnloaded;
        }
        
        private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            if (scene.buildIndex == 0)
            {
                return;
            }
            
            Map map = Map.Active;
            if (map == null)
            {
                return;
            }
            
            Vector2[] waypointLLs = map.waypoints.ToArray();
            float latLen = map.originLength.x;
            float lonLen = map.originLength.y;
            float originLat = map.origin.x;
            float originLon = map.origin.y;

            /*
                latitude = (transform.position.z + Utilities.Math.GetRandomNormal(0, latitudeNoise)) / map.originLength.x + map.origin.x,
                longitude = (transform.position.x + Utilities.Math.GetRandomNormal(0, longitudeNoise)) / map.originLength.y + map.origin.y,
            */

            foreach (Vector2 waypointLL in waypointLLs)
            {
                float lat = waypointLL.x;
                float lon = waypointLL.y;
                float latPos = (lat - originLat) * latLen;
                float lonPos = (lon - originLon) * lonLen;
                Vector3 waypointPos = new Vector3(lonPos, 0, latPos);

                GameObject waypointObject = Instantiate(waypointPrefab, waypointPos, Quaternion.identity);
                waypointObject.transform.SetParent(transform);
                waypointObjects.Add(waypointObject);
            }
        }

        private void OnSceneUnloaded(Scene scene)
        {            
            foreach (GameObject waypointObject in waypointObjects)
            {
                Destroy(waypointObject);
            }
            
            waypointObjects.Clear();
        }
    }
}