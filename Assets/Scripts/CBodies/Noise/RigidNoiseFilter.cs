using UnityEngine;

namespace CBodies.Noise
{
    public class RigidNoiseFilter : INoiseFilter
    {
        private readonly NoiseSettings.RigidNoiseSettings _noiseSettings;
        private readonly global::Noise _noise = new global::Noise();

        public RigidNoiseFilter(NoiseSettings.RigidNoiseSettings noiseSettings)
        {
            _noiseSettings = noiseSettings;
        }

        public float Evaluate(Vector3 point)
        {
            float noiseValue = 0;
            float frequency = _noiseSettings.baseRoughness;
            float amplitude = 1;
            float weight = 1;

            for (int i = 0; i < _noiseSettings.numLayers; i++)
            {
                float v = 1 - Mathf.Abs(_noise.Evaluate(point * frequency + _noiseSettings.centre));
                v *= v;
                v *= weight;
                weight = Mathf.Clamp01(v * _noiseSettings.weightMultiplier); // this way we will have more detailed peaks, going up
            
                noiseValue += v * amplitude;
                frequency *= _noiseSettings.roughness;
                amplitude *= _noiseSettings.persistance;
            }

            noiseValue = noiseValue - _noiseSettings.minValue; 
        
            return noiseValue * _noiseSettings.strength;
        }
    }
}