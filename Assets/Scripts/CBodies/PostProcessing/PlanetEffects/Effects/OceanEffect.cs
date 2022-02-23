using Game;
using UnityEngine;

namespace CBodies.PostProcessing.PlanetEffects.Effects
{
    public class OceanEffect {
        
        private SunShadowCaster _light;
        private Material _material;

        public void UpdateSettings (CBodyGenerator generator, Shader shader) 
        {
            // if (!generator.cBodySettings.ocean.hasPhysicChanged) return;
            // generator.cBodySettings.ocean.hasPhysicChanged = false;
            
            if (_material == null || _material.shader != shader) {
                _material = new Material (shader);
            }

            if (_light == null) {
                _light = Object.FindObjectOfType<SunShadowCaster>();
            }

            Vector3 centre = generator.transform.position;
            float radius = generator.GetOceanRadius ();
            _material.SetVector ("oceanCentre", centre);
            _material.SetFloat ("oceanRadius", radius);

            _material.SetFloat ("planetScale", generator.BodyScale);
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
            generator.cBodySettings.ocean.SetOceanProperties(_material, generator.cBodySettings.atmosphere.GetSettings().hasAtmosphere);
        }

        public Material GetMaterial () {
            return _material;
        }
    }
}