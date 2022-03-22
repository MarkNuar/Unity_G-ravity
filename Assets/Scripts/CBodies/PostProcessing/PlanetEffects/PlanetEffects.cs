using System;
using System.Collections.Generic;
using System.Linq;
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
		public Shader atmosphereShader;
		public Shader ringShader;
		public bool displayOceans = true;
		public bool displayAtmospheres = true;
		public bool displayRings = true;

		[SerializeField] private List<EffectHolder> _effectHolders = null;
		private List<float> _sortDistances;

		private List<Material> _postProcessingMaterials;

		public bool active = true;

		public override void Render (RenderTexture source, RenderTexture destination) {
			var materials = GetMaterials ();
			//Debug.LogError("materials: " + materials.Count);
			CustomPostProcessing.RenderMaterials (source, destination, materials);
		}

		
		// TODO: 
		// use three events (onSystemCreation, onSystemUpdate, onSystemDestroy) to update effect holders 
		
		public override void Awake_ScriptableObject()
		{
			base.Awake_ScriptableObject();
			_effectHolders?.Clear();
		}

		private void Init () {
		
			if (_effectHolders == null)
			{
				var generators = new List<CBodyGenerator>(FindObjectsOfType<CBodyGenerator> ());
				_effectHolders = new List<EffectHolder>(generators.Count);
				foreach (CBodyGenerator t in generators)
				{
					_effectHolders.Add (new EffectHolder (t));
				}
			}
			
			if (_effectHolders.Count != SystemUtils.Instance.currentSystemSettings.cBodiesSettings.Count)
			{
				var prevGenerators = _effectHolders.Select(e => e.generator).ToList();
				var curGenerators = new List<CBodyGenerator>(FindObjectsOfType<CBodyGenerator> ());
				// CBody added
				if (curGenerators.Count > _effectHolders.Count)
				{
					foreach (CBodyGenerator t in curGenerators.Where(
						t => !prevGenerators.Contains(t)))
					{
						_effectHolders.Add (new EffectHolder (t));
					}
				}
				// CBody removed
				else
				{
					foreach (CBodyGenerator t in prevGenerators.Where(
						t => !curGenerators.Contains(t)))
					{
						foreach (EffectHolder holder in _effectHolders.Where(h => h.generator == t))
						{
							_effectHolders.Remove(holder);
							break;
						}
					}
				}
			}

			_postProcessingMaterials ??= new List<Material>();

			_sortDistances ??= new List<float>();
			
			_sortDistances.Clear ();
			_postProcessingMaterials.Clear ();
		}

		private List<Material> GetMaterials () {

			if (!active) {
				return null;
			}
			Init ();
			
			//Debug.LogError("effect holders: " +_effectHolders.Count);

			if (_effectHolders.Count > 0) {
				Camera cam = GameManager.Instance.GetMainCamera();
				Vector3 camPos = cam.transform.position;

				SortFarToNear (camPos);

				foreach (EffectHolder effectHolder in _effectHolders)
				{
					Material underwaterMaterial = null;
					// Oceans
					if (displayOceans) 
					{
						if (effectHolder.oceanEffect != null)
						{
							effectHolder.oceanEffect.UpdateSettings (effectHolder.generator, oceanShader);
							float camDstFromCentre = (camPos - effectHolder.generator.transform.position).magnitude;
							if (camDstFromCentre < effectHolder.generator.GetOceanRadius ()) {
								underwaterMaterial = effectHolder.oceanEffect.GetMaterial ();
							} else {
								_postProcessingMaterials.Add (effectHolder.oceanEffect.GetMaterial ());
							}
						}
					}
					
					// Atmospheres
					if (displayAtmospheres) 
					{
						if (effectHolder.atmosphereEffect != null) 
						{
							effectHolder.atmosphereEffect.UpdateSettings (effectHolder.generator, atmosphereShader);
							_postProcessingMaterials.Add (effectHolder.atmosphereEffect.GetMaterial ());
						}
					}
					
					// Rings 
					if (displayRings)
					{
						if (effectHolder.ringEffect != null)
						{
							effectHolder.ringEffect.UpdateSettings(effectHolder.generator, ringShader);
							_postProcessingMaterials.Add(effectHolder.ringEffect.GetMaterial());
						}
					}

					if (underwaterMaterial != null) 
					{
						_postProcessingMaterials.Add (underwaterMaterial);
					}
				}
			}

			return _postProcessingMaterials;
		}
		
		private class EffectHolder {
			public CBodyGenerator generator;
			public OceanEffect oceanEffect;
			public AtmosphereEffect atmosphereEffect;
			public RingEffect ringEffect;
			
			public EffectHolder (CBodyGenerator generator) {
				this.generator = generator;
				switch (generator.cBodySettings.cBodyType)
				{
					case CBodySettings.CBodyType.Planet:
						oceanEffect = new OceanEffect ();
						atmosphereEffect = new AtmosphereEffect ();
						break;
					case CBodySettings.CBodyType.Gaseous:
						ringEffect = new RingEffect();
						//atmosphereEffect = new AtmosphereEffect ();
						break;
					case CBodySettings.CBodyType.Moon : 
						break;
					case CBodySettings.CBodyType.Star:
						break;
					default:
						throw new ArgumentOutOfRangeException();
				}
			}

			public float DstFromSurface (Vector3 viewPos) {
				return Mathf.Max (0, (generator.transform.position - viewPos).magnitude - generator.BodyScale);
			}
		}

		private void SortFarToNear (Vector3 viewPos)
		{
			foreach (EffectHolder t in _effectHolders)
			{
				float dstToSurface = t.DstFromSurface (viewPos);
				_sortDistances.Add (dstToSurface);
			}

			for (var i = 0; i < _effectHolders.Count - 1; i++) {
				for (var j = i + 1; j > 0; j--) {
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