using System;
using JetBrains.Annotations;
using UI.Menu.SystemEditing;
using UnityEngine;
using Random = UnityEngine.Random;

namespace CBodies.Data
{
    [Serializable]
    public class PhysicsData
    {
        [CanBeNull] private CBody _observer;
        
        [SerializeField] private float currentRadius;
        [SerializeField] private float currentSurfaceGravity;
        [SerializeField] private Vector3 currentInitialPosition; 
        [SerializeField] private Vector3 currentInitialVelocity;

        public float radius
        {
            get => currentRadius;
            set
            {
                if(Math.Abs(currentRadius - value) < Mathf.Epsilon)
                    return;
                currentRadius = value;
                if(_observer) _observer.OnPhysicsUpdate(this);
            }
        }

        public float surfaceGravity
        {
            get => currentSurfaceGravity;
            set
            {
                if(Math.Abs(currentSurfaceGravity - value) < Mathf.Epsilon)
                    return;
                currentSurfaceGravity = value;
                if(_observer) _observer.OnPhysicsUpdate(this);
            }
        }

        public Vector3 initialPosition
        {
            get => currentInitialPosition;
            set
            {
                if ((currentInitialPosition - value).magnitude < Mathf.Epsilon)
                    return;
                currentInitialPosition = value;
                if(_observer) _observer.OnPhysicsUpdate(this);
            }
        }
        
        public Vector3 initialVelocity
        {
            get => currentInitialVelocity;
            set
            {
                if ((currentInitialVelocity - value).magnitude < Mathf.Epsilon)
                    return;
                currentInitialVelocity = value;
                if(_observer) _observer.OnPhysicsUpdate(this);
            }
        }
        
        public void Init(Vector3 pos)
        {
            currentRadius = Random.Range(0.5f, 10);
            currentSurfaceGravity = Random.Range(5, 20);
            currentInitialPosition = pos;
            currentInitialVelocity = Vector3.up * ParameterValues.minVelocity; // TODO MULTIPLY BY INITIAL VELOCITY RANDOM SCALE
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
