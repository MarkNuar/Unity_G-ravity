using System;
using CBodies.Noise;
using UnityEngine;

namespace CBodies.Settings.Shape
{
    [Serializable][CreateAssetMenu]
    public class MoonShape : Shape
    {
        // MEMENTO 
        [SerializeField] public MoonShapeSettings shapeSettings;
        
        protected override void SetShapeData()
        {
            PRNG prng = new PRNG (shapeSettings.seed);
            
            shapeSettings.shapeNoise.SetComputeValues (heightMapCompute, prng, "_shape");
            shapeSettings.ridgeNoise.SetComputeValues (heightMapCompute, prng, "_ridge");
            shapeSettings.ridgeNoise2.SetComputeValues (heightMapCompute, prng, "_ridge2");
        }

        private void SetShapeNoiseSettings (PRNG prng, bool randomizeValues) {
            const string suffix = "_shape";
            if (randomizeValues) {
                var num = prng.Range(0,3);
                SimpleNoiseSettings randomizedShapeNoise = new SimpleNoiseSettings () {
                    numLayers = 4, lacunarity = 2, persistence = 0.5f
                };

                if (num == 1) { // Minor deformation
                    randomizedShapeNoise.elevation = Mathf.Lerp (0.2f, 3, prng.ValueBiasLower (0.3f));
                    randomizedShapeNoise.scale = prng.Range (1.5f, 2.5f);
                } else { // Major deformation
                    randomizedShapeNoise.elevation = Mathf.Lerp (3, 8, prng.ValueBiasLower (0.4f));
                    randomizedShapeNoise.scale = prng.Range (0.3f, 1);
                }
                // Assign settings
                randomizedShapeNoise.SetComputeValues (heightMapCompute, prng, suffix);

            } else {
                shapeSettings.shapeNoise.SetComputeValues (heightMapCompute, prng, suffix);
            }
        }

        private void SetRidgeNoiseSettings (PRNG prng, bool randomizeValues) {
            const string ridgeSuffix = "_ridge";
            const string detailSuffix = "_ridge2";
            if (randomizeValues) {
                // Randomize ridge mask
                var randomizedMaskNoise = new SimpleNoiseSettings () {
                    numLayers = 4, lacunarity = 2, persistence = 0.6f, elevation = 1
                };
                randomizedMaskNoise.scale = prng.Range (0.5f, 2f);

                // Randomize ridge noise
                var randomizedRidgeNoise = new RidgeNoiseSettings () {
                    numLayers = 4, power = 3, gain = 1, peakSmoothing = 2
                };

                randomizedRidgeNoise.elevation = Mathf.Lerp (1, 7, prng.ValueBiasLower (0.3f));
                randomizedRidgeNoise.scale = prng.Range (1f, 3f);
                randomizedRidgeNoise.lacunarity = prng.Range (1f, 5f);
                randomizedRidgeNoise.persistence = 0.42f;
                randomizedRidgeNoise.power = prng.Range (1.5f, 3.5f);

                randomizedRidgeNoise.SetComputeValues (heightMapCompute, prng, ridgeSuffix);
                randomizedMaskNoise.SetComputeValues (heightMapCompute, prng, detailSuffix);

            } else {
                shapeSettings.ridgeNoise.SetComputeValues (heightMapCompute, prng, ridgeSuffix);
                shapeSettings.ridgeNoise2.SetComputeValues (heightMapCompute, prng, detailSuffix);
            }
        }

        // MEMENTO PATTERN
        public override void InitSettings()
        {
            if (Observers == null) return;
            foreach (ICBodyObserver o in Observers)
            {
                o.OnShapeUpdate();
            }
        }

        public override ShapeSettings GetSettings()
        {
            return shapeSettings;
        }

        public override void SetSettings(ShapeSettings ss)
        {
            shapeSettings = (MoonShapeSettings)ss;

            if (Observers == null) return;
            foreach (ICBodyObserver o in Observers)
            {
                o.OnShapeUpdate();
            }
        }

        [Serializable]
        public class MoonShapeSettings : ShapeSettings
        {
            [Header("Noise settings")] 
            public SimpleNoiseSettings shapeNoise = new SimpleNoiseSettings();
            public RidgeNoiseSettings ridgeNoise = new RidgeNoiseSettings();
            public RidgeNoiseSettings ridgeNoise2 = new RidgeNoiseSettings();
        }
    }
}