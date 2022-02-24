using System;
using CBodies.Noise;
using UnityEngine;
using Utilities;
using Random = UnityEngine.Random;

namespace CBodies.Settings.Shading
{
    [Serializable][CreateAssetMenu]
    public class MoonShading : Shading
    {
        private ComputeBuffer _pointBuffer;
        
        // MEMENTO
        [SerializeField] protected MoonShadingSettings shadingSettings;

        public override void InitSettings()
        {
            if (Observers == null) return;
            foreach (ICBodyObserver o in Observers)
            {
                o.OnShadingUpdate();
            }
        }

        public override ShadingSettings GetSettings()
        {
            return shadingSettings;
        }

        public override void SetSettings(ShadingSettings ss)
        {
            shadingSettings = (MoonShadingSettings)ss;
            
            if (Observers == null) return;
            foreach (ICBodyObserver o in Observers)
            {
                o.OnShadingUpdate();
            }
        }

        public override void SetSurfaceProperties(Material material, Vector2 heightMinMax, float bodyScale, float oceanLevel)
        {
            material.SetVector ("heightMinMax", heightMinMax);
            material.SetFloat ("bodyScale", bodyScale);
            
            SetCraterBiomesSettings(material);

            if (shadingSettings.randomize)
            {
                SetRandomColors ();
                ApplyColours (material, shadingSettings.randomMoonColors);
            }
            else
            {
                ApplyColours (material, shadingSettings.baseMoonColors);
            }
            
            shadingSettings.mainColor = Color.gray;
        }
        
        private void SetRandomColors () {
            PRNG random = new PRNG (shadingSettings.seed);
            if (shadingSettings.realisticColors)
            {
                var deltaH = random.Range(shadingSettings.colorHRange.x, shadingSettings.colorHRange.y);
                MoonColors colors = shadingSettings.baseMoonColors;
                
                shadingSettings.randomMoonColors.primaryColorA =
                    ColorHelper.TweakHSV(colors.primaryColorA, deltaH, 0, 0);
                shadingSettings.randomMoonColors.secondaryColorA = 
                    ColorHelper.TweakHSV(colors.secondaryColorA, deltaH, 0, 0);
                shadingSettings.randomMoonColors.primaryColorB = 
                    ColorHelper.TweakHSV(colors.primaryColorB, deltaH, 0, 0);
                shadingSettings.randomMoonColors.secondaryColorB = 
                    ColorHelper.TweakHSV(colors.secondaryColorB, deltaH, 0, 0);
            }
            else
            {
                shadingSettings.randomMoonColors.primaryColorA = 
                    ColorHelper.Random (random, 0.45f, 0.6f, 0.7f, 0.8f); 
                shadingSettings.randomMoonColors.secondaryColorA = 
                    ColorHelper.TweakHSV (shadingSettings.randomMoonColors.primaryColorA, random.SignedValue () * 0.2f, random.SignedValue () * 0.15f, random.Range (-0.25f, -0.2f));
                shadingSettings.randomMoonColors.primaryColorB = 
                    ColorHelper.Random (random, 0.45f, 0.6f, 0.7f, 0.8f); 
                shadingSettings.randomMoonColors.secondaryColorB = 
                    ColorHelper.TweakHSV (shadingSettings.randomMoonColors.primaryColorB, random.SignedValue () * 0.2f, random.SignedValue () * 0.15f, random.Range (-0.25f, -0.2f));
            }
        }
        
        void ApplyColours (Material material, MoonColors colors) {
            material.SetColor ("primaryColorA", colors.primaryColorA);
            material.SetColor ("secondaryColorA", colors.secondaryColorA);
            material.SetColor ("primaryColorB", colors.primaryColorB);
            material.SetColor ("secondaryColorB", colors.secondaryColorB);
        }
        
        private void SetCraterBiomesSettings (Material material) {
            PRNG random = new PRNG (shadingSettings.seed);
            
            Vector4 biomesValues = new Vector4 (
                random.SignedValueBiasExtremes (0.3f),
                random.SignedValueBiasExtremes (0.3f) * 0.4f,
                random.SignedValueBiasExtremes (0.3f) * 0.3f,
                random.SignedValueBiasCentre (0.3f) * .7f
            );
            material.SetVector ("_RandomBiomeValues", biomesValues);
            var warpStrength = random.SignedValueBiasCentre (.65f) * 30;
            material.SetFloat ("_BiomeBlendStrength", random.Range (2f, 12) + Mathf.Abs (warpStrength) / 2);
            material.SetFloat ("_BiomeWarpStrength", warpStrength);
        }
        
        protected override void SetShadingDataComputeProperties () {
            SetRandomPoints ();
            SetShadingNoise ();
        }

        private void SetShadingNoise () {
            const string biomesWarpNoiseSuffix = "_biomesWarp";
            const string detailWarpNoiseSuffix = "_detailWarp";
            const string detailNoiseSuffix = "_detail";

            PRNG prng = new PRNG (shadingSettings.seed);
            PRNG prng2 = new PRNG (shadingSettings.seed);
            if (shadingSettings.randomize) {
                // warp 1
                SimpleNoiseSettings randomizedBiomesWarpNoise = new SimpleNoiseSettings
                {
                    elevation = prng.Range (0.8f, 3f),
                    scale = prng.Range (1f, 3f)
                };
                randomizedBiomesWarpNoise.SetComputeValues (shadingDataCompute, prng2, biomesWarpNoiseSuffix);

                // warp 2
                SimpleNoiseSettings randomizedDetailWarpNoise = new SimpleNoiseSettings
                {
                    scale = prng.Range (1f, 3f),
                    elevation = prng.Range (1f, 5f)
                };
                randomizedDetailWarpNoise.SetComputeValues (shadingDataCompute, prng2, detailWarpNoiseSuffix);

                shadingSettings.detailNoise.SetComputeValues (shadingDataCompute, prng2, detailNoiseSuffix);

            } else {
                shadingSettings.biomesWarpNoise.SetComputeValues (shadingDataCompute, prng2, biomesWarpNoiseSuffix);
                shadingSettings.detailWarpNoise.SetComputeValues (shadingDataCompute, prng2, detailWarpNoiseSuffix);
                shadingSettings.detailNoise.SetComputeValues (shadingDataCompute, prng2, detailNoiseSuffix);
            }
        }

        private void SetRandomPoints () {
            Random.InitState (shadingSettings.seed);

            var randomizedNumPoints = shadingSettings.numBiomesPoints;
            if (shadingSettings.randomize) {
                randomizedNumPoints = Random.Range (15, 50);
            }
            Random.InitState (shadingSettings.seed);
            var randomPoints = new Vector4[randomizedNumPoints];
            for (int i = 0; i < randomPoints.Length; i++) {
                var point = Random.onUnitSphere;
                var radius = Mathf.Lerp (shadingSettings.radiusMinMax.x, shadingSettings.radiusMinMax.y, Random.value);
                randomPoints[i] = new Vector4 (point.x, point.y, point.z, radius);
            }
            ComputeHelper.CreateAndSetBuffer<Vector4> (ref _pointBuffer, randomPoints, shadingDataCompute, "points");
            shadingDataCompute.SetInt ("numRandomPoints", randomPoints.Length);
        }

        [Serializable]
        public class MoonShadingSettings : ShadingSettings
        {
            public MoonColors baseMoonColors;
            public MoonColors randomMoonColors;
            public Vector2 colorHRange;

            public int numBiomesPoints;
            
            public Vector2 radiusMinMax = new Vector2 (0.02f, 0.1f);
            
            public SimpleNoiseSettings biomesWarpNoise = new SimpleNoiseSettings();
            public SimpleNoiseSettings detailNoise = new SimpleNoiseSettings();
            public SimpleNoiseSettings detailWarpNoise = new SimpleNoiseSettings();
        }
        
        [Serializable]
        public struct MoonColors
        {
            public Color primaryColorA;
            public Color secondaryColorA;
            public Color primaryColorB;
            public Color secondaryColorB;
        }
    }
}