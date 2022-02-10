using System;
using UnityEngine;
using Utilities;

namespace CBodies.Settings.PostProcessing.Ocean
{
    [Serializable][CreateAssetMenu]
    public class Ocean : ScriptableObject 
    {
        private static System.Random _prng = new System.Random ();
        
        // MEMENTO
        [SerializeField] private OceanSettings oceanSettings;
        
        [Header ("Waves Normals")]
        public Texture2D waveNormalA;
        public Texture2D waveNormalB;
        
        
        private bool _settingsUpToDate = false;
        
        public void SetOceanProperties (Material material) {
            if(_settingsUpToDate) return;
            
            if(!oceanSettings.hasOcean) return;
            
            material.SetFloat ("depthMultiplier", oceanSettings.depthMultiplier);
            material.SetFloat ("alphaMultiplier", oceanSettings.alphaMultiplier);

            material.SetTexture ("waveNormalA", waveNormalA);
            material.SetTexture ("waveNormalB", waveNormalB);
            material.SetFloat ("waveStrength", oceanSettings.waveStrength);
            material.SetFloat ("waveNormalScale", oceanSettings.waveScale);
            material.SetFloat ("waveSpeed", oceanSettings.waveSpeed);
            material.SetFloat ("smoothness", oceanSettings.smoothness);
            material.SetVector ("params", oceanSettings.testParams);
            
            if (oceanSettings.randomizeShading) {
                PRNG random = new PRNG (oceanSettings.shadingSeed);
                if (oceanSettings.realisticColors)
                {
                    float deltaH = random.Range(-oceanSettings.hueRange, oceanSettings.hueRange);
                    oceanSettings.randomShallowCol =
                        ColorHelper.TweakHSV(oceanSettings.baseShallowCol, deltaH, 0, 0);
                    oceanSettings.randomDepthCol =
                        ColorHelper.TweakHSV(oceanSettings.baseDepthCol, deltaH, 0, 0);
                }
                else
                {
                    oceanSettings.randomShallowCol = Color.HSVToRGB (random.Value (), random.Range (0.6f, 0.8f), random.Range (0.65f, 1));
                    oceanSettings.randomDepthCol = ColorHelper.TweakHSV (oceanSettings.randomShallowCol,
                        random.SignedValue() * 0.2f,
                        random.SignedValue() * 0.2f,
                        random.Range (-0.5f, -0.4f)
                    );
                }
                
                material.SetColor ("colA", oceanSettings.randomShallowCol);
                material.SetColor ("colB", oceanSettings.randomDepthCol);
                material.SetColor ("specularCol", Color.white);
            } else {
                material.SetColor ("colA", oceanSettings.baseShallowCol);
                material.SetColor ("colB", oceanSettings.baseDepthCol);
                material.SetColor ("specularCol", oceanSettings.specularCol);
            }

            _settingsUpToDate = true;
        }

        [Serializable]
        public class OceanSettings
        {
            public bool randomizeShading;
            public bool randomizeHeight;
            public int shadingSeed = 0;
            public int heightSeed = 0;

            public bool realisticColors = true;
            public float hueRange = 0.05f;
            
            public bool hasOcean;

            private float _oceanLevel = 0.55f;
            public float baseOceanLevel = 0.5f;
            public float minOceanLevel = 0.3f;
            public float maxOceanLevel = 0.85f;

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
                    return _oceanLevel;
                return 0;
                // return _oceanLevel;
            }

            public void UpdateOceanLevel()
            {
                Debug.Log("Updating ocean level");
                if (randomizeHeight)
                {
                    PRNG random = new PRNG (heightSeed);
                    _oceanLevel = random.Range(minOceanLevel, maxOceanLevel);
                }
                else
                {
                    _oceanLevel = baseOceanLevel;
                }
            }

            public void RandomizeShading(bool rand)
            {
                randomizeShading = rand;
                shadingSeed = rand ? _prng.Next(-10000, 10000) : 0;
            }
            
            public void RandomizeShape(bool rand)
            {
                randomizeHeight = rand;
                heightSeed = rand ? _prng.Next(-10000, 10000) : 0;
            }
        }
        
        // MEMENTO PATTERN
        public void InitSettings()
        {
            _settingsUpToDate = false;
        }
        public OceanSettings GetSettings()
        {
            return oceanSettings;
        }

        public void SetSettings(OceanSettings ps)
        {
            oceanSettings = ps;
            oceanSettings.UpdateOceanLevel();
            _settingsUpToDate = false;
        }
    }
}