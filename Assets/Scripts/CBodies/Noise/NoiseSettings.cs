using UnityEngine;

namespace CBodies.Noise
{
    public class NoiseSettings
    {
        public enum FilterType { Simple, Rigid };
        public FilterType filterType;
    
        public SimpleNoiseSettings simpleNoiseSettings;
        public RigidNoiseSettings rigidNoiseSettings;

        public class SimpleNoiseSettings
        {
            public float strength = 1;
            [Range(1,8)] public int numLayers = 1;
            public float baseRoughness = 1;
            public float roughness = 2; // at each layer, the frequency if increased by roughness factor
            public float persistance = 0.5f;  // at each layer, the amplitude is decreased by persistance factor
            public Vector3 centre = Vector3.zero;
            public float minValue = 0;
        }

        public class RigidNoiseSettings : SimpleNoiseSettings
        {
            public float weightMultiplier = 0.8f;
        }    
    
    
    
    }
}