using System.Collections.Generic;
using Scrabby.ScriptableObjects;
using UnityEngine;

namespace Scrabby
{
    public class Barrel : MonoBehaviour
    {
        public bool randomizeColor = false;

        void Start()
        {
            if (!randomizeColor)
            {
                return;
            }

            Material[] materials = GetComponentsInChildren<Renderer>()[0].materials;

            // The 0th material is the "plastic" materila
            Material plasticMaterial = materials[0];

            // Set the color of the plastic material to a random color from the list in the map
            List<Color> colors = Map.Active.barrelColors;
            if (colors.Count > 0)
            {
                int randomIndex = Random.Range(0, colors.Count);
                Color randomColor = colors[randomIndex];
                plasticMaterial.color = randomColor;
            }
            else
            {
                Debug.LogWarning("No barrel colors available in the map.");
            }
        }
    }
}
