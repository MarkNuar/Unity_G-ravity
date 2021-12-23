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

            [Header("Noise settings")] 
            public SimpleNoiseSettings continentNoise = new SimpleNoiseSettings();

            public SimpleNoiseSettings maskNoise = new SimpleNoiseSettings();

            public RidgeNoiseSettings ridgeNoise = new RidgeNoiseSettings();
            
            public Vector4 testParams = Vector4.zero;
        }
    }
}