using System;
using JetBrains.Annotations;
using UI.Menu.SystemEditing;
using UnityEngine;

namespace CBodies.Settings.Physics
{
    [Serializable][CreateAssetMenu]
    public class Physics : ScriptableObject
    {
        // OBSERVER
        [CanBeNull] private CBodyGenerator _observer;
        // MEMENTO
        [SerializeReference] private PhysicsSettings physicsSettings;
        
        
        // OBSERVER PATTERN
        public void Subscribe(CBodyGenerator observer)
        {
            _observer = observer;
        }

        public void Unsubscribe()
        {
            _observer = null;
        }
        
        public void RandomInitialize(Vector3 pos)
        {
            physicsSettings = new PhysicsSettings
            {
                radius = ParameterValues.minRadius,
                surfaceGravity = ParameterValues.minGravity,
                initialPosition = pos,
                initialVelocity = Vector3.up * ParameterValues.minVelocity
                // ...
            };
            if (_observer)
                _observer.OnPhysicsUpdate();
        }

        [Serializable]
        public class PhysicsSettings
        {
            public float radius;
            public float surfaceGravity;
            public Vector3 initialPosition; 
            public Vector3 initialVelocity;
        }
        
        
        // MEMENTO PATTERN
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
