﻿using System.Collections.Generic;
using CBodies.PostProcessing.PlanetEffects.Effects;
using CBodies.Settings;
using UnityEngine;

/*
	Responsible for rendering oceans and atmospheres as post processing effect
*/

namespace CBodies.PostProcessing.PlanetEffects
{
	[CreateAssetMenu (menuName = "PostProcessing/PlanetEffects")]
	public class PlanetEffects : PostProcessingEffect {

		public Shader oceanShader;
		//public Shader atmosphereShader;
		public bool displayOceans = true;
		//public bool displayAtmospheres = true;

		List<EffectHolder> _effectHolders;
		List<float> _sortDistances;

		List<Material> _postProcessingMaterials;
		bool active = true;

		public override void Render (RenderTexture source, RenderTexture destination) {
			List<Material> materials = GetMaterials ();
			CustomPostProcessing.RenderMaterials (source, destination, materials);
		}

		void Init () {
			if (_effectHolders == null || _effectHolders.Count == 0 || 
			    // new cBody added, happens only in edit mode
			    _effectHolders.Count != SystemUtils.Instance.currentSystemSettings.cBodiesSettings.Count) {
				var generators = FindObjectsOfType<CBodyGenerator> ();
				_effectHolders = new List<EffectHolder> (generators.Length);
				foreach (CBodyGenerator t in generators)
				{
					_effectHolders.Add (new EffectHolder (t));
				}
			}
			if (_postProcessingMaterials == null) {
				_postProcessingMaterials = new List<Material> ();
			}
			if (_sortDistances == null) {
				_sortDistances = new List<float> ();
			}
			_sortDistances.Clear ();
			_postProcessingMaterials.Clear ();
		}

		public List<Material> GetMaterials () {

			if (!active) {
				return null;
			}
			Init ();

			if (_effectHolders.Count > 0) {
				Camera cam = GameManager.Instance.GetMainCamera();
				Vector3 camPos = cam.transform.position;

				SortFarToNear (camPos);

				foreach (EffectHolder effectHolder in _effectHolders)
				{
					Material underwaterMaterial = null;
					// Oceans
					if (displayOceans) {
						if (effectHolder.oceanEffect != null) {

							effectHolder.oceanEffect.UpdateSettings (effectHolder.generator, oceanShader);

							float camDstFromCentre = (camPos - effectHolder.generator.transform.position).magnitude;
							if (camDstFromCentre < effectHolder.generator.GetOceanRadius ()) {
								underwaterMaterial = effectHolder.oceanEffect.GetMaterial ();
							} else {
								_postProcessingMaterials.Add (effectHolder.oceanEffect.GetMaterial ());
							}
						}
					}
					// // Atmospheres
					// if (displayAtmospheres) {
					// 	if (effectHolder.atmosphereEffect != null) {
					// 		effectHolder.atmosphereEffect.UpdateSettings (effectHolder.generator);
					// 		postProcessingMaterials.Add (effectHolder.atmosphereEffect.GetMaterial ());
					// 	}
					// }

					if (underwaterMaterial != null) {
						_postProcessingMaterials.Add (underwaterMaterial);
					}
				}
			}

			return _postProcessingMaterials;
		}

		public class EffectHolder {
			public CBodyGenerator generator;
			public OceanEffect oceanEffect;
			//public AtmosphereEffect atmosphereEffect;

			public EffectHolder (CBodyGenerator generator) {
				this.generator = generator;
				if (generator.cBodySettings.cBodyType == CBodySettings.CBodyType.Rocky && generator.cBodySettings.ocean.GetSettings().hasOcean) {
					oceanEffect = new OceanEffect ();
				}
				// if (generator.body.shading.hasAtmosphere && generator.body.shading.atmosphereSettings) {
				// 	atmosphereEffect = new AtmosphereEffect ();
				// }
			}

			public float DstFromSurface (Vector3 viewPos) {
				return Mathf.Max (0, (generator.transform.position - viewPos).magnitude - generator.BodyScale);
			}
		}

		void SortFarToNear (Vector3 viewPos) {
			for (int i = 0; i < _effectHolders.Count; i++) {
				float dstToSurface = _effectHolders[i].DstFromSurface (viewPos);
				_sortDistances.Add (dstToSurface);
			}

			for (int i = 0; i < _effectHolders.Count - 1; i++) {
				for (int j = i + 1; j > 0; j--) {
					if (_sortDistances[j - 1] < _sortDistances[j]) {
						float tempDst = _sortDistances[j - 1];
						var temp = _effectHolders[j - 1];
						_sortDistances[j - 1] = _sortDistances[j];
						_sortDistances[j] = tempDst;
						_effectHolders[j - 1] = _effectHolders[j];
						_effectHolders[j] = temp;
					}
				}
			}
		}
	}
}