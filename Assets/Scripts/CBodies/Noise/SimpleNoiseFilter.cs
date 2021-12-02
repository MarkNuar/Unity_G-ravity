using UnityEngine;

namespace CBodies.Noise
{
    [System.Serializable]
    public class SimpleNoiseSettings {
        public int numLayers = 4;
        public float lacunarity = 2;
        public float persistence = 0.5f;
        public float scale = 1;
        public float elevation = 1;
        public float verticalShift = 0;
        public Vector3 offset;

        // Set values using exposed settings
        public void SetComputeValues (ComputeShader cs, PRNG prng, string varSuffix) {
            SetComputeValues (cs, prng, varSuffix, scale, elevation, persistence);
        }

        // Set values using custom scale and elevation
        public void SetComputeValues (ComputeShader cs, PRNG prng, string varSuffix, float cScale, float cElevation) {
            SetComputeValues (cs, prng, varSuffix, cScale, cElevation, persistence);
        }

        // Set values using custom scale, elevation and persistance
        public void SetComputeValues (ComputeShader cs, PRNG prng, string varSuffix, float cScale, float cElevation, float cPersistence) {
            Vector3 seededOffset = new Vector3 (prng.Value (), prng.Value (), prng.Value ()) * prng.Value () * 10000;

            float[] noiseParams = {
                // [0]
                seededOffset.x + offset.x,
                seededOffset.y + offset.y,
                seededOffset.z + offset.z,
                numLayers,
                // [1]
                cPersistence,
                lacunarity,
                cScale,
                cElevation,
                // [2]
                verticalShift
            };

            cs.SetFloats ("noiseParams" + varSuffix, noiseParams);
        }
    }
}