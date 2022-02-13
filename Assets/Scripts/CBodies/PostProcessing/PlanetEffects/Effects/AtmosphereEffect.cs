using Game;
using UnityEngine;

namespace CBodies.PostProcessing.PlanetEffects.Effects
{
    public class AtmosphereEffect {

        private SunShadowCaster _light;
        private Material _material;

        public void UpdateSettings (CBodyGenerator generator, Shader shader)
        {
            // if (!generator.cBodySettings.atmosphere.hasPhysicChanged) return;
            // generator.cBodySettings.atmosphere.hasPhysicChanged = false;

            if (_material == null || _material.shader != shader) {
                _material = new Material (shader);
            }

            if (_light == null) {
                _light = Object.FindObjectOfType<SunShadowCaster>();
            }

            _material.SetVector ("planetCentre", generator.transform.position);
            _material.SetFloat ("oceanRadius", generator.GetOceanRadius ());
            _material.SetFloat ("planetRadius", generator.BodyScale);

            if (_light) {
                
                //_material.SetVector ("dirToSun", -_light.transform.forward);
                //TODO LIGHTING 
                Vector3 dirFromPlanetToSun = (_light.transform.position - generator.transform.position).normalized;
                //Debug.Log(dirFromPlanetToSun);
                _material.SetVector ("dirToSun", dirFromPlanetToSun);
                
            } else {
                _material.SetVector ("dirToSun", Vector3.up);
                Debug.Log ("No SunShadowCaster found");
            }
            
            //generator.shading.SetAtmosphereProperties (material);
            generator.cBodySettings.atmosphere.SetAtmosphereProperties (_material, generator.BodyScale);
        }

        public Material GetMaterial () {
            return _material;
        }
    }
}