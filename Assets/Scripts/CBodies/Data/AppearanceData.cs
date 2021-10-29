using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using UnityEngine;
using UnityEngine.Serialization;
using Random = UnityEngine.Random;

namespace CBodies.Data
{
    [Serializable]
    public class AppearanceData
    {
        [CanBeNull] private CBody _observer;

        [SerializeField] private Color currentColor;
        [SerializeField] private int currentResolution;
        
        public Color color
        {
            get => currentColor;
            set
            {
                if(currentColor == value)
                    return;
                currentColor = value;

                if(_observer)
                    _observer.OnAppearanceUpdate(this, AppearanceUpdateType.Material);
            }
        }

        public int resolution
        {
            get => currentResolution;
            set
            {
                if(currentResolution == value)
                    return;
                currentResolution = value;

                if(_observer)
                    _observer.OnAppearanceUpdate(this, AppearanceUpdateType.Mesh);
            }
        }

        public void Init(int res)
        {
            currentColor = Random.ColorHSV();
            currentResolution = res;
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
    }
}
