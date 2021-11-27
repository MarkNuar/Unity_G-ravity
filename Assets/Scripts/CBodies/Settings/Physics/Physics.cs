using System;
using JetBrains.Annotations;
using UI.Menu.SystemEditing;
using UnityEngine;

namespace CBodies.Settings.Physics
{
    [Serializable]
    public class Physics
    {
        [CanBeNull] private CBodyGenerator _observer;

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
                if(_observer) _observer.OnPhysicsUpdate();
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
                if(_observer) _observer.OnPhysicsUpdate();
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
                if(_observer) _observer.OnPhysicsUpdate();
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
                if(_observer) _observer.OnPhysicsUpdate();
            }
        }
        
        public void Init(Vector3 pos)
        {
            //currentRadius = Random.Range(1f, ParameterValues.maxRadius);
            currentRadius = ParameterValues.minRadius; // Min radius?
            currentSurfaceGravity = ParameterValues.minGravity;
            currentInitialPosition = pos;
            currentInitialVelocity = Vector3.up * ParameterValues.minVelocity;
        }
        
        public void Subscribe(CBodyGenerator observer)
        {
            _observer = observer;
        }

        public void Unsubscribe()
        {
            _observer = null;
        }
    }
}
