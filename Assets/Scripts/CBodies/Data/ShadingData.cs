using System;
using JetBrains.Annotations;
using UnityEngine;
using Random = UnityEngine.Random;

namespace CBodies.Data
{
    [Serializable]
    public class ShadingData
    {
        [CanBeNull] private CBody _observer;

        [SerializeField] private Color currentColor;

        public Color color
        {
            get => currentColor;
            set
            {
                if(currentColor == value)
                    return;
                currentColor = value;

                if(_observer)
                    _observer.OnMaterialUpdate();
            }
        }
        

        public void Init()
        {
            currentColor = Random.ColorHSV();
            // ...
        }

        public void Subscribe(CBody observer)
        {
            _observer = observer;
        }
        
        public void Unsubscribe()
        {
            _observer = null;
        }
        
        public enum MaterialUpdateType
        {
            Material, // color
            All // ?
        }
    }
}
