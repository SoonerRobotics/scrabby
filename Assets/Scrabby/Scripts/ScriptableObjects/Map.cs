using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

namespace Scrabby.ScriptableObjects
{
    [CreateAssetMenu(fileName = "Map", menuName = "SCR/Map")]
    [Serializable]
    public class Map : ScriptableObject
    {
        public static Map Active;

        public string id;
        public new string name;
        public string category;
        public int sceneIndex;
        public List<Robot> availableRobots;
        public List<Color> barrelColors;

        public Vector2 origin;
        public Vector2 originLength;
    }
}
