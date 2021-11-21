using System;
using JetBrains.Annotations;
using UI.Menu.SystemEditing;
using UnityEngine;
using Random = UnityEngine.Random;


namespace CBodies.Data
{
    [Serializable]
    public class ShapeData
    {
        [CanBeNull] private CBody _observer;

        // Mesh resolution
        [SerializeField] private int currentResolution;
        // Max height of the mountains of the CBody
        [SerializeField] private float currentMountainsHeight;
        
        public int resolution
        {
            get => currentResolution;
            set
            {
                if(currentResolution == value)
                    return;
                currentResolution = value;
    
                if(_observer)
                    _observer.OnMeshUpdate();
            }
        }
        
        public float mountainsHeight
        {
            get => currentMountainsHeight;
            set
            {
                if(Math.Abs(currentMountainsHeight - value) < Mathf.Epsilon)
                    return;
                currentMountainsHeight = value;
                if(_observer) _observer.OnMeshUpdate();
            }
        }
    
        public void Init(int res)
        {
            currentResolution = res;

            mountainsHeight = Random.Range(ParameterValues.minMountainsHeight, ParameterValues.maxMountainsHeight);
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
            
        public enum MeshUpdateType
        {
            Mesh, // shape
            All
        }
    }
}
