using UnityEngine;

namespace Scrabby.Utilities
{
    public static class Math
    {
        public static float GetRandomNormal(float mean, float stdDev)
        {
            var u1 = 1.0f - Random.value;
            var u2 = 1.0f - Random.value;
            var randStdNormal = Mathf.Sqrt(-2.0f * Mathf.Log(u1)) *  Mathf.Sin(2.0f * Mathf.PI * u2);
            return stdDev * randStdNormal;
        }
    }
}