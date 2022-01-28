﻿using Game;
using UnityEngine;

namespace CBodies.PostProcessing.PlanetEffects.Effects
{
    public class RingEffect
    {
        private Light _light;
        private Material _material;

        public void UpdateSettings (CBodyGenerator generator, Shader shader) 
        {
            // if (!generator.cBodySettings.ring.hasPhysicChanged) return;
            // generator.cBodySettings.ring.hasPhysicChanged = false;
            
            if (_material == null || _material.shader != shader) {
                _material = new Material (shader);
            }

            if (_light == null) {
                _light = Object.FindObjectOfType<SunShadowCaster> ()?.GetComponent<Light> ();
            }

            Vector3 centre = generator.transform.position;
            float radius = generator.cBodySettings.physics.GetSettings().radius;
            _material.SetVector ("c_body_center", centre);
            _material.SetFloat ("c_body_radius", radius);
            
            if (_light)
            {
                _material.SetInt("receive_shadow", 1);
                _material.SetVector ("light_direction", -_light.transform.forward);
            } else {
                _material.SetInt("receive_shadow", 0);
            }
            
            generator.cBodySettings.ring.SetRingProperties (_material);
        }

        public Material GetMaterial () {
            return _material;
        }
    }
}