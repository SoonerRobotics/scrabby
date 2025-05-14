using Scrabby.ScriptableObjects;
using Scrabby.Utilities;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Scrabby.Gameplay
{
    public class RobotSpawner : MonoSingleton<RobotSpawner>
    {
        private SpawnPoint _spawnPoint;
        
        private void Start()
        {            
            SceneManager.sceneLoaded += OnSceneLoaded;
        }
        
        private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            if (scene.buildIndex == 0)
            {
                return;
            }

            _spawnPoint = FindFirstObjectByType<SpawnPoint>();
            var spawnPoint = _spawnPoint == null ? Vector3.zero : _spawnPoint.transform.position;
            var robot = Robot.Active;
            var robotGameObject = Instantiate(robot.prefab);
            robotGameObject.transform.position = spawnPoint;
            
            // Choose a random direction (either 0 rotation or 180 rotation)
            // var direction = Random.Range(0, 2) == 0 ? 0 : 180;
            var direction = 0; // IGVC/AutoNav is now always north
            robotGameObject.transform.rotation = Quaternion.Euler(0, direction, 0);
        }
    }
}