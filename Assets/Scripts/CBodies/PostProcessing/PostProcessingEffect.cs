﻿using UnityEngine;

namespace CBodies.PostProcessing
{
    public abstract class PostProcessingEffect : ScriptableObject {

        protected Material material;

        public virtual Material GetMaterial () {
            return null;
        }

        public virtual void ReleaseBuffers () {

        }

        public abstract void Render (RenderTexture source, RenderTexture destination);
    }
}