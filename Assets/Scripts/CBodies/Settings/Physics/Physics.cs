using System;
using Game.UI.Menu.SystemEditing;
using JetBrains.Annotations;
using UnityEngine;

namespace CBodies.Settings.Physics
{
    [Serializable][CreateAssetMenu]
    public class Physics : ScriptableObject
    {
        // OBSERVER
        [CanBeNull] private CBodyGenerator _observer;
        // MEMENTO
        [SerializeField] private PhysicsSettings physicsSettings;
        
        
        // OBSERVER PATTERN
        public void Subscribe(CBodyGenerator observer)
        {
            _observer = observer;
        }

        public void Unsubscribe()
        {
            _observer = null;
        }
        // END OBSERVER
        
        
        [Serializable]
        public class PhysicsSettings
        {
            public float minRadius = 1f;
            public float maxRadius = 40f;
            public float radius = 5f;
            
            public float minSurfaceGravity = 0.1f;
            public float maxSurfaceGravity = 20f;
            public float surfaceGravity = 7;

            public float minRotationSpeed = -30f;
            public float maxRotationSpeed = 30f;
            public float rotationSpeed = 0;

            public Vector3 initialPosition = Vector3.zero;

            public float minSpeed = 0.1f;
            public float maxSpeed = 20f;
            public Vector3 initialVelocity = Vector3.up * 5;
        }
        
        
        // MEMENTO PATTERN
        public void InitSettings()
        {
            if(_observer)
                _observer.OnPhysicsUpdate();
        }
        public PhysicsSettings GetSettings()
        {
            return physicsSettings;
        }

        public void SetSettings(PhysicsSettings ps)
        {
            physicsSettings = ps;
            if(_observer)
                _observer.OnPhysicsUpdate();
        }
    }
}
