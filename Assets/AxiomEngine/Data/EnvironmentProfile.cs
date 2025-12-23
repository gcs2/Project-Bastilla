using UnityEngine;

namespace RPGPlatform.Data
{
    [CreateAssetMenu(fileName = "NewEnvironmentProfile", menuName = "Axiom/Environment Profile")]
    public class EnvironmentProfile : ScriptableObject
    {
        [Header("Floor Settings")]
        public Vector3 FloorScale = new Vector3(5, 1, 5);
        public Color FloorTint = new Color(0.6f, 0.8f, 0.7f);
        public Texture2D FloorTexture;

        [Header("Lighting (Cinematic)")]
        public Color PrimaryLightColor = new Color(0f, 0.8f, 0.8f);
        public float LightIntensity = 50f;
        public float LightRange = 30f;

        [Header("Props (Modular)")]
        public GameObject StallPrefab; // If null, defaults to Cube
        public int StallCount = 3;
        public float StallSpread = 8f;
    }
}
