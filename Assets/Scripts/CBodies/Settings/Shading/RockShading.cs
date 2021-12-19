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
        [SerializeReference] protected RockShadingSettings shadingSettings;

        // MEMENTO PATTERN
        public override void InitSettings()
        {
            shadingSettings = new RockShadingSettings
            {
                //color = Random.ColorHSV()
            };
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

            if (shadingSettings.randomize) {
                SetRandomColours (material);
                ApplyColours (material, shadingSettings.randomizedCols);
            } else {
                ApplyColours (material, shadingSettings.customizedCols);
            }
        }
        
        void SetRandomColours (Material material) {
            PRNG random = new PRNG (shadingSettings.seed);
            //randomizedCols.shoreCol = ColourHelper.Random (random, 0.3f, 0.7f, 0.4f, 0.8f);
            shadingSettings.randomizedCols.flatColLowA = ColorHelper.Random (random, 0.45f, 0.6f, 0.7f, 0.8f);
            shadingSettings.randomizedCols.flatColHighA = ColorHelper.TweakHSV (
                shadingSettings.randomizedCols.flatColLowA,
                random.SignedValue () * 0.2f,
                random.SignedValue () * 0.15f,
                random.Range (-0.25f, -0.2f)
            );

            shadingSettings.randomizedCols.flatColLowB = ColorHelper.Random (random, 0.45f, 0.6f, 0.7f, 0.8f);
            shadingSettings.randomizedCols.flatColHighB = ColorHelper.TweakHSV (
                shadingSettings.randomizedCols.flatColLowB,
                random.SignedValue () * 0.2f,
                random.SignedValue () * 0.15f,
                random.Range (-0.25f, -0.2f)
            );

            shadingSettings.randomizedCols.shoreColLow = ColorHelper.Random (random, 0.2f, 0.3f, 0.9f, 1);
            shadingSettings.randomizedCols.shoreColHigh = ColorHelper.TweakHSV (
                shadingSettings.randomizedCols.shoreColLow,
                random.SignedValue () * 0.2f,
                random.SignedValue () * 0.2f,
                random.Range (-0.3f, -0.2f)
            );

            shadingSettings.randomizedCols.steepLow = ColorHelper.Random (random, 0.3f, 0.7f, 0.4f, 0.6f);
            shadingSettings.randomizedCols.steepHigh = ColorHelper.TweakHSV (
                shadingSettings.randomizedCols.steepLow,
                random.SignedValue () * 0.2f,
                random.SignedValue () * 0.2f,
                random.Range (-0.35f, -0.2f)
            );
        }
        
        void ApplyColours (Material material, EarthColours colours) {
            material.SetColor ("_ShoreLow", colours.shoreColLow);
            material.SetColor ("_ShoreHigh", colours.shoreColHigh);

            material.SetColor ("_FlatLowA", colours.flatColLowA);
            material.SetColor ("_FlatHighA", colours.flatColHighA);

            material.SetColor ("_FlatLowB", colours.flatColLowB);
            material.SetColor ("_FlatHighB", colours.flatColHighB);

            material.SetColor ("_SteepLow", colours.steepLow);
            material.SetColor ("_SteepHigh", colours.steepHigh);
        }
        

        [Serializable]
        public class RockShadingSettings : ShadingSettings
        {
            public EarthColours customizedCols;
            public EarthColours randomizedCols;

            [Header ("Shading Data")]
            public SimpleNoiseSettings detailWarpNoise = new SimpleNoiseSettings();
            public SimpleNoiseSettings detailNoise = new SimpleNoiseSettings();
            public SimpleNoiseSettings largeNoise = new SimpleNoiseSettings();
            public SimpleNoiseSettings smallNoise = new SimpleNoiseSettings();
        }
        
        [System.Serializable]
        public struct EarthColours {
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