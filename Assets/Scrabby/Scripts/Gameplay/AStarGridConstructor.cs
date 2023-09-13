using System.Collections.Generic;
using UnityEngine;

namespace Scrabby.Gameplay
{
    public class AStarGridConstructor : MonoBehaviour
    {
        private const int MapWidth = 10;
        private const int MapHeight = 10;
        public GameObject wallPrefab;
        
        public List<Vector2> path = new();

        private void Start()
        {
            // Go from -MAP_WIDTH to MAP_WIDTH, and -MAP_HEIGHT to MAP_HEIGHT to create a grid of walls
            // ignore 0,0 and any points within Path
            for (var x = -MapWidth; x < MapWidth; x++)
            {
                for (var y = -MapHeight; y < MapHeight; y++)
                {
                    if (x == 0 && y == 0)
                    {
                        continue;
                    }

                    if (path.Contains(new Vector2(x, y)))
                    {
                        continue;
                    }

                    var wall = Instantiate(wallPrefab, new Vector3(x, 0, y), Quaternion.identity);
                    wall.transform.parent = transform;
                }
            }
        }
        
        private void OnDrawGizmos()
        {
            Gizmos.color = Color.red;
            foreach (var point in path)
            {
                Gizmos.DrawSphere(new Vector3(point.x, 0, point.y), 0.5f);
            }
            
            Gizmos.color = Color.black;
            for (var x = -MapWidth; x < MapWidth; x++)
            {
                for (var y = -MapHeight; y < MapHeight; y++)
                {
                    if (x == 0 && y == 0)
                    {
                        continue;
                    }

                    if (path.Contains(new Vector2(x, y)))
                    {
                        continue;
                    }

                    Gizmos.DrawCube(new Vector3(x, 0, y), Vector3.one);
                }
            }
        }
    }
}
