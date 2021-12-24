using System;
using JetBrains.Annotations;
using UnityEngine;
using Utilities;

namespace CBodies.Settings.Ocean
{
    [Serializable][CreateAssetMenu]
    public class Ocean : ScriptableObject {
        
        // // OBSERVER
        // [CanBeNull] private CBodyGenerator _observer;
        
        private static System.Random _prng = new System.Random ();
        
        // MEMENTO
        [SerializeField] private OceanSettings oceanSettings;
        
        [Header ("Waves Normals")]
        public Texture2D waveNormalA;
        public Texture2D waveNormalB;
        
        
        public void SetOceanProperties(Material oceanMaterial)
        {
            if (oceanSettings.hasOcean)
            {
                SetProperties(oceanMaterial, oceanSettings.seed, oceanSettings.randomize);
            }
        }
        
        public void SetProperties (Material material, int seed, bool randomize) {
            material.SetFloat ("depthMultiplier", oceanSettings.depthMultiplier);
            material.SetFloat ("alphaMultiplier", oceanSettings.alphaMultiplier);

            material.SetTexture ("waveNormalA", waveNormalA);
            material.SetTexture ("waveNormalB", waveNormalB);
            material.SetFloat ("waveStrength", oceanSettings.waveStrength);
            material.SetFloat ("waveNormalScale", oceanSettings.waveScale);
            material.SetFloat ("waveSpeed", oceanSettings.waveSpeed);
            material.SetFloat ("smoothness", oceanSettings.smoothness);
            material.SetVector ("params", oceanSettings.testParams);

            if (randomize) {
                var random = new PRNG (seed);
                oceanSettings.randomShallowCol = Color.HSVToRGB (random.Value (), random.Range (0.6f, 0.8f), random.Range (0.65f, 1));
                oceanSettings.randomDepthCol = ColorHelper.TweakHSV (oceanSettings.randomShallowCol,
                    random.SignedValue() * 0.2f,
                    random.SignedValue() * 0.2f,
                    random.Range (-0.5f, -0.4f)
                );

                material.SetColor ("colA", oceanSettings.randomShallowCol);
                material.SetColor ("colB", oceanSettings.randomDepthCol);
                material.SetColor ("specularCol", Color.white);
            } else {
                material.SetColor ("colA", oceanSettings.baseShallowCol);
                material.SetColor ("colB", oceanSettings.baseDepthCol);
                material.SetColor ("specularCol", oceanSettings.specularCol);
            }
        }

        [Serializable]
        public class OceanSettings
        {
            public bool randomize;
            public int seed = _prng.Next(-10000, 10000);
        
            public bool hasOcean;
            [Range (0, 1)]
            [SerializeField] private float oceanLevel = 0.55f;
            
            public float depthMultiplier = 10;
            public float alphaMultiplier = 70;
            public Color baseShallowCol;
            public Color baseDepthCol;
            public Color randomShallowCol = Color.black;
            public Color randomDepthCol = Color.black;
            public Color specularCol = Color.white;

            [Range (0, 1)]
            public float waveStrength = 0.15f;
            public float waveScale = 15;
            public float waveSpeed = 0.5f;

            [Range (0, 1)]
            public float smoothness = 0.92f;
            public Vector4 testParams;

            public float GetOceanLevel()
            {
                if (hasOcean)
                    return oceanLevel;
                return 0;
            }
        }
        
        // MEMENTO PATTERN
        public void InitSettings()
        {
            // if(_observer)
            //     _observer.OnPhysicsUpdate();
        }
        public OceanSettings GetSettings()
        {
            return oceanSettings;
        }

        public void SetSettings(OceanSettings ps)
        {
            oceanSettings = ps;
            if (ps.randomize)
            {
                // Randomize ocean colors
                
            }
            // if(_observer)
            //     _observer.OnPhysicsUpdate();
        }
    }
}