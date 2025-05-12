﻿using System.Collections.Generic;
using System.Linq;
using Scrabby.ScriptableObjects;
using Scrabby.Utilities;
using UnityEngine;

namespace Scrabby.State
{
    public class ScrabbyState : MonoSingleton<ScrabbyState>
    {
        public List<Robot> robots;
        public List<Map> maps;
        public bool movementEnabled = true;
        public bool canMoveManually = false;
        public bool randomizeSeed;
        public int randomSeed;

        protected override void Init()
        {
            randomSeed = Random.state.GetHashCode();
        }

        public Robot GetRobotById(string id)
        {
            if (robots.Any(x => x.id == id)) return robots.First(x => x.id == id);

            Debug.LogWarning("Robot with id " + id + " not found.");
            return null;
        }

        public Map GetMapById(string id)
        {
            if (maps.Any(x => x.id == id)) return maps.First(x => x.id == id);

            Debug.LogWarning("Map with id " + id + " not found.");
            return null;
        }
    }
}