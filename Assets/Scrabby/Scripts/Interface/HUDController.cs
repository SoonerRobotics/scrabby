using TMPro;
using UnityEngine;
using UnityEngine.Serialization;

namespace Scrabby.Interface
{
    public class HUDController : MonoBehaviour
    {
        private static HUDController Instance { get; set; }
        private static string[] _cardinalDirections = { "N", "NE", "E", "SE", "S", "SW", "W", "NW", "N" };

        [Header("Text Fields")]
        public TMP_Text degreesText;
        public TMP_Text radiansText;
        public TMP_Text milesPerHourText;
        [FormerlySerializedAs("metersPerHourText")] public TMP_Text metersPerSecondText;
        public TMP_Text xText;
        public TMP_Text yText;
        public TMP_Text latitudeText;
        public TMP_Text longitudeText;
        
        private void Start()
        {
            if (Instance != null)
            {
                Destroy(gameObject);
            }
            
            Instance = this;
        }

        public static void UpdateDegrees(float degrees)
        {
            var direction = _cardinalDirections[Mathf.RoundToInt((degrees % 360) / 45)];
            Instance.degreesText.text = $"{direction} {degrees:F0}°";
        }
        
        public static void UpdateRadians(float radians)
        {
            Instance.radiansText.text = $"{radians:F2} rad";
        }
        
        public static void UpdateMilesPerHour(float milesPerHour)
        {
            Instance.milesPerHourText.text = $"{milesPerHour:F1} mph";
        }
        
        public static void UpdateMetersPerSecond(float metersPerSecond)
        {
            Instance.metersPerSecondText.text = $"{metersPerSecond:F1} m/s";
        }
        
        public static void UpdateX(float x)
        {
            Instance.xText.text = $"X: {x:F2}";
        }
        
        public static void UpdateY(float y)
        {
            Instance.yText.text = $"Y: {y:F2}";
        }
        
        public static void UpdateLatitude(float latitude)
        {
            Instance.latitudeText.text = $"Lat: {latitude:F5}°";
        }
        
        public static void UpdateLongitude(float longitude)
        {
            Instance.longitudeText.text = $"Lon: {longitude:F5}°";
        }
    }
}
