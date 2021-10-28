using UnityEngine;

namespace CBodies
{
    [RequireComponent (typeof (Rigidbody))]
    public class CBody : GravityObject
    {
        public float radius;
        public float surfaceGravity;
        public Vector3 initialVelocity;
        public string bodyName = "Unnamed";
        
        private Transform _meshHolder;

        public Vector3 Velocity { get; private set; }
        public float Mass { get; private set; }
        private Rigidbody _rb;
        
        private void Awake () {
            _rb = GetComponent<Rigidbody> ();
        }

        private void Start()
        {
            OnValuesUpdated();
            _rb.mass = Mass;
            _rb.isKinematic = true;
            Velocity = initialVelocity;
        }

        public void UpdateVelocity (Vector3 acceleration, float timeStep) {
            Velocity += acceleration * timeStep;
        }

        public void UpdatePosition (float timeStep) {
            _rb.MovePosition (_rb.position + Velocity * timeStep);
        }

        public void OnValuesUpdated () {
            Mass = surfaceGravity * radius * radius / Constants.GravitationalConstant;
            _meshHolder = transform.GetChild (0);
            _meshHolder.localScale = Vector3.one * radius;
            gameObject.name = bodyName;
        }

        public Rigidbody Rigidbody => _rb;

        public Vector3 Position => _rb.position;
    }
}
