using System;
using CBodies.Noise;
using UnityEngine;
using Utilities;
using Random = UnityEngine.Random;

namespace CBodies.Settings.Shading
{
    [Serializable][CreateAssetMenu]
    public class RockShading : Shading
    {
        // MEMENTO
        [SerializeField] protected RockShadingSettings shadingSettings;

        // MEMENTO PATTERN
        public override void InitSettings()
        {
            SetColors();
            // todo: eventually remove customized colors if players cannot choose directly every single color
            // shadingSettings.baseColors = shadingSettings.randomColors;
            if(Observer)
                Observer.OnShadingUpdate();
        }
        public override ShadingSettings GetSettings()
        {
            return shadingSettings;
        }
        public override void SetSettings(ShadingSettings ss)
        {
            shadingSettings = (RockShadingSettings)ss;
            if(Observer)
                Observer.OnShadingUpdate();
        }
        
        protected override void SetShadingDataComputeProperties () {
            PRNG random = new PRNG (shadingSettings.seed);
            shadingSettings.detailNoise.SetComputeValues (shadingDataCompute, random, "_detail");
            shadingSettings.detailWarpNoise.SetComputeValues (shadingDataCompute, random, "_detailWarp");
            shadingSettings.largeNoise.SetComputeValues (shadingDataCompute, random, "_large");
            shadingSettings.smallNoise.SetComputeValues (shadingDataCompute, random, "_small");
        }
        
        public override void SetTerrainProperties (Material material, Vector2 heightMinMax, float bodyScale) 
        {
            material.SetVector ("heightMinMax", heightMinMax);
            material.SetFloat ("oceanLevel", shadingSettings.oceanLevel);
            material.SetFloat ("bodyScale", bodyScale);

            if (shadingSettings.randomize)
            {
                SetColors ();
                ApplyColours (material, shadingSettings.randomColors);
            }
            else
            {
                ApplyColours (material, shadingSettings.baseColors);
            }
        }

        public override void SetOceanProperties(Material oceanMaterial)
        {
            if (shadingSettings.hasOcean && oceanSettings)
            {
                oceanSettings.SetProperties(oceanMaterial, shadingSettings.seed, shadingSettings.randomize);
            }
        }

        private void SetColors () {
            PRNG random = new PRNG (shadingSettings.seed);
            //randomizedCols.shoreCol = ColourHelper.Random (random, 0.3f, 0.7f, 0.4f, 0.8f);
            shadingSettings.randomColors.flatColLowA = ColorHelper.Random (random, 0.45f, 0.6f, 0.7f, 0.8f);
            shadingSettings.randomColors.flatColHighA = ColorHelper.TweakHSV (
                shadingSettings.randomColors.flatColLowA,
                random.SignedValue () * 0.2f,
                random.SignedValue () * 0.15f,
                random.Range (-0.25f, -0.2f)
            );

            shadingSettings.randomColors.flatColLowB = ColorHelper.Random (random, 0.45f, 0.6f, 0.7f, 0.8f);
            shadingSettings.randomColors.flatColHighB = ColorHelper.TweakHSV (
                shadingSettings.randomColors.flatColLowB,
                random.SignedValue () * 0.2f,
                random.SignedValue () * 0.15f,
                random.Range (-0.25f, -0.2f)
            );

            shadingSettings.randomColors.shoreColLow = ColorHelper.Random (random, 0.2f, 0.3f, 0.9f, 1);
            shadingSettings.randomColors.shoreColHigh = ColorHelper.TweakHSV (
                shadingSettings.randomColors.shoreColLow,
                random.SignedValue () * 0.2f,
                random.SignedValue () * 0.2f,
                random.Range (-0.3f, -0.2f)
            );

            shadingSettings.randomColors.steepLow = ColorHelper.Random (random, 0.3f, 0.7f, 0.4f, 0.6f);
            shadingSettings.randomColors.steepHigh = ColorHelper.TweakHSV (
                shadingSettings.randomColors.steepLow,
                random.SignedValue () * 0.2f,
                random.SignedValue () * 0.2f,
                random.Range (-0.35f, -0.2f)
            );
        }
        
        void ApplyColours (Material material, EarthColors colors) {
            material.SetColor ("_ShoreLow", colors.shoreColLow);
            material.SetColor ("_ShoreHigh", colors.shoreColHigh);

            material.SetColor ("_FlatLowA", colors.flatColLowA);
            material.SetColor ("_FlatHighA", colors.flatColHighA);

            material.SetColor ("_FlatLowB", colors.flatColLowB);
            material.SetColor ("_FlatHighB", colors.flatColHighB);

            material.SetColor ("_SteepLow", colors.steepLow);
            material.SetColor ("_SteepHigh", colors.steepHigh);
        }
        

        [Serializable]
        public class RockShadingSettings : ShadingSettings
        {
            public EarthColors baseColors;
            public EarthColors randomColors;

            [Header ("Shading Data")]
            public SimpleNoiseSettings detailWarpNoise = new SimpleNoiseSettings();
            public SimpleNoiseSettings detailNoise = new SimpleNoiseSettings();
            public SimpleNoiseSettings largeNoise = new SimpleNoiseSettings();
            public SimpleNoiseSettings smallNoise = new SimpleNoiseSettings();
        }
        
        [System.Serializable]
        public struct EarthColors {
            public Color shoreColLow;
            public Color shoreColHigh;
            public Color flatColLowA;
            public Color flatColHighA;
            public Color flatColLowB;
            public Color flatColHighB;
            public Color steepLow;
            public Color steepHigh;
        }
    }
}