using UnityEngine;

namespace CBodies
{
    //[RequireComponent (typeof (Rigidbody))]
    // Observer of SystemData
    public class CBody : GravityObject
    {
        private Vector3 velocity { get; set; }
        public float mass;
        public Vector3 position => _rb.position;
        public Rigidbody Rigidbody => _rb;

        private Rigidbody _rb;

        public CBodyGenerator cBodyGenerator;
        public Material surfaceMaterial;

        private void Awake () {
            _rb = gameObject.AddComponent<Rigidbody> ();
            _rb.isKinematic = true;

            cBodyGenerator = gameObject.AddComponent<CBodyGenerator>();
            cBodyGenerator.cBody = this;
        }

        public void UpdateVelocity (Vector3 acceleration, float timeStep) {
            velocity += acceleration * timeStep;
        }

        public void UpdatePosition (float timeStep) {
            _rb.MovePosition (_rb.position + velocity * timeStep);
        }
    }
}