using System;
using System.Collections.Generic;
using Game.UI.Menu.SystemEditing;
using JetBrains.Annotations;
using UnityEngine;

namespace CBodies.Settings.Physics
{
    [Serializable][CreateAssetMenu]
    public class Physics : ScriptableObject
    {
        // OBSERVER
        [CanBeNull] private List<ICBodyObserver> _observers = new List<ICBodyObserver>();
        // MEMENTO
        [SerializeField] private PhysicsSettings physicsSettings;
        
        
        // OBSERVER PATTERN
        public void Subscribe(ICBodyObserver observer)
        {
            _observers?.Add(observer);
        }

        public void UnsubscribeAll()
        {
            _observers = null;
        }
        // END OBSERVER
        
        
        [Serializable]
        public class PhysicsSettings
        {
            public float minRadius = 1f;
            public float maxRadius = 40f;
            public float radius = 5f;
            
            public float minSurfaceGravity = 5f;
            public float maxSurfaceGravity = 50f;
            public float surfaceGravity = 10;
            
            public Vector3 initialPosition = Vector3.zero;

            public float minSpeed = 0.1f;
            public float maxSpeed = 20f;
            public Vector3 initialVelocity = Vector3.up * 5;
        }
        
        
        // MEMENTO PATTERN
        public void InitSettings()
        {
            if (_observers == null) return;
            foreach (ICBodyObserver o in _observers)
            {
                o.OnPhysicsUpdate();
            }
        }
        public PhysicsSettings GetSettings()
        {
            return physicsSettings;
        }

        public void SetSettings(PhysicsSettings ps)
        {
            physicsSettings = ps;
            
            if (_observers == null) return;
            foreach (ICBodyObserver o in _observers)
            {
                o.OnPhysicsUpdate();
            }
        }
    }
}
