using System.Collections.Generic;
using UnityEngine;

namespace CBodies.PostProcessing
{
	[ImageEffectAllowedInSceneView]
    public class CustomPostProcessing : MonoBehaviour
    {
        public PostProcessingEffect[] effects;
		private Shader _defaultShader;
		private Material _defaultMat;
		private readonly List<RenderTexture> _temporaryTextures = new List<RenderTexture> ();
		// public bool debugOceanMask;

		public event System.Action<RenderTexture> onPostProcessingComplete;
		public event System.Action<RenderTexture> onPostProcessingBegin;

		void Init () {
			if (_defaultShader == null) {
				_defaultShader = Shader.Find ("Unlit/Texture");
			}
			_defaultMat = new Material (_defaultShader);
		}

		[ImageEffectOpaque]
		private void OnRenderImage (RenderTexture initialSource, RenderTexture finalDestination) {
			onPostProcessingBegin?.Invoke (finalDestination);
			
			Init ();

			_temporaryTextures.Clear ();

			RenderTexture currentSource = initialSource;
			RenderTexture currentDestination = null;

			if (effects != null) {
				for (int i = 0; i < effects.Length; i++) {
					PostProcessingEffect effect = effects[i];
					if (effect != null) {
						if (i == effects.Length - 1) {
							// Final effect, so render into final destination texture
							currentDestination = finalDestination;
						} else {
							// Get temporary texture to render this effect into
							currentDestination = GetTemporaryRenderTexture (finalDestination);
							_temporaryTextures.Add (currentDestination); //
						}

						effect.Render (currentSource, currentDestination); // render the effect
						currentSource = currentDestination; // output texture of this effect becomes input for next effect
					}
				}
			}

			// In case dest texture was not rendered into (due to being provided a null effect), copy current src to dest
			if (currentDestination != finalDestination) {
				Graphics.Blit (currentSource, finalDestination, _defaultMat);
			}

			// Release temporary textures
			foreach (RenderTexture t in _temporaryTextures)
			{
				RenderTexture.ReleaseTemporary (t);
			}

			// Trigger post processing complete event
			onPostProcessingComplete?.Invoke (finalDestination);

		}

		// Helper function for blitting a list of materials
		public static void RenderMaterials (RenderTexture source, RenderTexture destination, List<Material> materials) {
			List<RenderTexture> temporaryTextures = new List<RenderTexture> ();

			RenderTexture currentSource = source;
			RenderTexture currentDestination = null;

			if (materials != null) {
				for (int i = 0; i < materials.Count; i++) {
					Material material = materials[i];
					if (material != null) {

						if (i == materials.Count - 1) { // last material
							currentDestination = destination;
						} else {
							// get temporary texture to render this effect into
							currentDestination = GetTemporaryRenderTexture (destination);
							temporaryTextures.Add (currentDestination);
						}
						Graphics.Blit (currentSource, currentDestination, material);
						currentSource = currentDestination;
					}
				}
			}

			// In case dest texture was not rendered into (due to being provided a null material), copy current src to dest
			if (currentDestination != destination) {
				Graphics.Blit (currentSource, destination, new Material (Shader.Find ("Unlit/Texture")));
			}
			// Release temporary textures
			for (int i = 0; i < temporaryTextures.Count; i++) {
				RenderTexture.ReleaseTemporary (temporaryTextures[i]);
			}
		}

		public static RenderTexture GetTemporaryRenderTexture (RenderTexture template) {
			return RenderTexture.GetTemporary (template.descriptor);
		}
    }
}