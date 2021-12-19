using System;
using CBodies.Noise;
using UnityEngine;
using Random = UnityEngine.Random;

namespace CBodies.Settings.Shape
{
    [Serializable][CreateAssetMenu]
    public class RockShape : Shape
    {
        // MEMENTO
        [SerializeField] public RockShapeSettings shapeSettings;
        
        protected override void SetShapeData () {
            PRNG prng = new PRNG (shapeSettings.seed);
            
            shapeSettings.continentNoise.SetComputeValues (heightMapCompute, prng, "_continents");
            shapeSettings.ridgeNoise.SetComputeValues (heightMapCompute, prng, "_mountains");
            shapeSettings.maskNoise.SetComputeValues (heightMapCompute, prng, "_mask");

            heightMapCompute.SetBool("hasOcean", shapeSettings.hasOcean);
            heightMapCompute.SetFloat ("oceanDepthMultiplier", shapeSettings.oceanDepthMultiplier);
            heightMapCompute.SetFloat ("oceanFloorDepth", shapeSettings.oceanFloorDepth);
            heightMapCompute.SetFloat ("oceanFloorSmoothing", shapeSettings.oceanFloorSmoothing);
            heightMapCompute.SetFloat ("mountainBlend", shapeSettings.mountainBlend);
            heightMapCompute.SetVector ("params", shapeSettings.testParams);
            //
        }
        
        // MEMENTO PATTERN
        public override void InitSettings()
        {
            if(Observer)
                Observer.OnShapeUpdate();
        }
        public override ShapeSettings GetSettings()
        {
            return shapeSettings;
        }
        public override void SetSettings (ShapeSettings ss)
        {
            shapeSettings = (RockShapeSettings)ss;
            if(Observer)
                Observer.OnShapeUpdate();
        }
        // END MEMENTO
        
        [Serializable]
        public class RockShapeSettings : ShapeSettings
        {
            [Header ("Continent settings")]
            public bool hasOcean = true;
            public float oceanDepthMultiplier = 5;
            public float oceanFloorDepth = 1.36f;
            public float oceanFloorSmoothing = 0.5f;
            public float mountainBlend = 1f; // Determines how smoothly the base of mountains blends into the terrain

            [Header("Noise settings")] public SimpleNoiseSettings continentNoise = new SimpleNoiseSettings
            {
                numLayers = 6,
                lacunarity = 2.12f,
                persistence = 0.5f,
                scale = 1,
                elevation = 2.64f,
                verticalShift = 0,
                offset = Vector3.zero,
            };

            public SimpleNoiseSettings maskNoise = new SimpleNoiseSettings
            {
                numLayers = 3,
                lacunarity = 1.66f,
                persistence = 0.55f,
                scale = 1.09f,
                elevation = 1,
                verticalShift = 0.2f,
                offset = Vector3.zero,
            };

            public RidgeNoiseSettings ridgeNoise = new RidgeNoiseSettings
            {
                numLayers = 5,
                lacunarity = 4,
                persistence = 0.5f,
                scale = 1.5f,
                power = 2.18f,
                elevation = 8.7f,
                gain = 0.8f,
                verticalShift = 0.09f,
                peakSmoothing = 1,
                offset = Vector3.zero
            };
            public Vector4 testParams = Vector4.zero;
        }
    }
}