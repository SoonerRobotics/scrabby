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
            DontDestroyOnLoad(this);
            
            SceneManager.sceneLoaded += OnSceneLoaded;
        }
        
        private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            if (scene.buildIndex == (int)SceneIndex.MainMenu)
            {
                return;
            }

            _spawnPoint = FindObjectOfType<SpawnPoint>();
            var spawnPoint = _spawnPoint == null ? Vector3.zero : _spawnPoint.transform.position;
            var robot = Robot.Active;
            var robotGameObject = Instantiate(robot.prefab);
            robotGameObject.transform.position = spawnPoint;
        }
    }
}