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
        private int _previousN = -1;
        
        // MEMENTO
        [SerializeField] protected RockShadingSettings shadingSettings;

        // MEMENTO PATTERN
        public override void InitSettings()
        {
            SetRandomColors();
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
        
        public override void SetSurfaceProperties (Material material, Vector2 heightMinMax, float bodyScale, float oceanLevel) 
        {
            material.SetVector ("heightMinMax", heightMinMax);
            material.SetFloat ("oceanLevel", oceanLevel);
            material.SetFloat ("bodyScale", bodyScale);

            if (shadingSettings.randomize)
            {
                SetRandomColors ();
                ApplyColours (material, shadingSettings.randomColors);
            }
            else
            {
                ApplyColours (material, shadingSettings.baseGreenColors);
            }
        }
        
        
        private void SetRandomColors () {
            PRNG random = new PRNG (shadingSettings.seed);
            if (shadingSettings.realisticColors)
            {
                var n = random.Range(0, 3);
                var deltaH = 0.0f;
                RockColors colors;
                switch(n)
                {
                    case 0:
                        colors = shadingSettings.baseGreenColors;
                        deltaH = random.Range(shadingSettings.greenHRange.x, shadingSettings.greenHRange.y);
                        break;
                    case 1:
                        colors = shadingSettings.baseRedColors;
                        deltaH = random.Range(shadingSettings.redHRange.x, shadingSettings.redHRange.y);
                        break;
                    case 2:
                        colors = shadingSettings.baseBlueColors;
                        deltaH = random.Range(shadingSettings.blueHRange.x, shadingSettings.blueHRange.y);
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                };

                shadingSettings.randomColors.shoreColLow =
                    ColorHelper.TweakHSV(colors.shoreColLow, deltaH, 0, 0);
                shadingSettings.randomColors.shoreColHigh = 
                    ColorHelper.TweakHSV(colors.shoreColHigh, deltaH, 0, 0);
                shadingSettings.randomColors.flatColLowA = 
                    ColorHelper.TweakHSV(colors.flatColLowA, deltaH, 0, 0);
                shadingSettings.randomColors.flatColHighA = 
                    ColorHelper.TweakHSV(colors.flatColHighA, deltaH, 0, 0);
                shadingSettings.randomColors.flatColLowB = 
                    ColorHelper.TweakHSV(colors.flatColLowB, deltaH, 0, 0);
                shadingSettings.randomColors.flatColHighB = 
                    ColorHelper.TweakHSV(colors.flatColHighB, deltaH, 0, 0);
                shadingSettings.randomColors.steepLow = 
                    ColorHelper.TweakHSV(colors.steepLow, deltaH, 0, 0);
                shadingSettings.randomColors.steepHigh = 
                    ColorHelper.TweakHSV(colors.steepHigh, deltaH, 0, 0);
            }
            else
            {
                shadingSettings.randomColors.flatColLowA = 
                    ColorHelper.Random (random, 0.45f, 0.6f, 0.7f, 0.8f); 
                shadingSettings.randomColors.flatColHighA = 
                    ColorHelper.TweakHSV (shadingSettings.randomColors.flatColLowA, random.SignedValue () * 0.2f, random.SignedValue () * 0.15f, random.Range (-0.25f, -0.2f));
                shadingSettings.randomColors.flatColLowB = 
                    ColorHelper.Random (random, 0.45f, 0.6f, 0.7f, 0.8f); 
                shadingSettings.randomColors.flatColHighB = 
                    ColorHelper.TweakHSV (shadingSettings.randomColors.flatColLowB, random.SignedValue () * 0.2f, random.SignedValue () * 0.15f, random.Range (-0.25f, -0.2f));
                shadingSettings.randomColors.shoreColLow = 
                    ColorHelper.Random (random, 0.2f, 0.3f, 0.9f, 1); 
                shadingSettings.randomColors.shoreColHigh =
                    ColorHelper.TweakHSV (shadingSettings.randomColors.shoreColLow, random.SignedValue () * 0.2f, random.SignedValue () * 0.2f, random.Range (-0.3f, -0.2f));
                shadingSettings.randomColors.steepLow = 
                    ColorHelper.Random (random, 0.3f, 0.7f, 0.4f, 0.6f); 
                shadingSettings.randomColors.steepHigh = 
                    ColorHelper.TweakHSV (shadingSettings.randomColors.steepLow, random.SignedValue () * 0.2f, random.SignedValue () * 0.2f, random.Range (-0.35f, -0.2f));
            }
        }
        
        void ApplyColours (Material material, RockColors colors) {
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
            public RockColors baseGreenColors;
            public RockColors baseRedColors;
            public RockColors baseBlueColors;

            public RockColors randomColors;
            
            public Vector2 greenHRange;
            public Vector2 redHRange;
            public Vector2 blueHRange;
                
            [Header ("Shading Data")]
            public SimpleNoiseSettings detailWarpNoise = new SimpleNoiseSettings();
            public SimpleNoiseSettings detailNoise = new SimpleNoiseSettings();
            public SimpleNoiseSettings largeNoise = new SimpleNoiseSettings();
            public SimpleNoiseSettings smallNoise = new SimpleNoiseSettings();
        }
        
        [System.Serializable]
        public struct RockColors {
            public Color shoreColLow;
            public Color shoreColHigh;
            public Color flatColLowA;
            public Color flatColHighA;
            public Color flatColLowB;
            public Color flatColHighB;
            public Color steepLow;
            public Color steepHigh;
        }


        [System.Serializable]
        public enum ColorType
        {
            Green,
            Red,
            Blue
        }
    }
}