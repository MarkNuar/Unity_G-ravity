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
        // END OBSERVER
        
        
        [Serializable]
        public class PhysicsSettings
        {
            public float radius = (ParameterValues.maxRadius - ParameterValues.minRadius)/3;
            public float surfaceGravity = ParameterValues.minGravity;
            public float rotationSpeed = ParameterValues.maxRotationSpeed - ParameterValues.minRotationSpeed + 5;
            public Vector3 initialPosition = Vector3.zero; 
            public Vector3 initialVelocity = Vector3.up * ParameterValues.minVelocity;
        }
        
        
        // MEMENTO PATTERN
        public void InitSettings()
        {
            physicsSettings = new PhysicsSettings();
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
