using UnityEngine;

namespace CBodies
{
    public class NBodySimulation : MonoBehaviour {
        CBody[] bodies;
        static NBodySimulation instance;
    
        void Awake () {

            bodies = FindObjectsOfType<CBody> ();
            Time.fixedDeltaTime = Constants.PhysicsTimeStep;
        }

        void FixedUpdate () {
            for (int i = 0; i < bodies.Length; i++) {
                Vector3 acceleration = CalculateAcceleration (bodies[i].position, bodies[i]);
                bodies[i].UpdateVelocity (acceleration, Constants.PhysicsTimeStep);
                //bodies[i].UpdateVelocity (bodies, Constants.PhysicsTimeStep);
            }

            for (int i = 0; i < bodies.Length; i++) {
                bodies[i].UpdatePosition (Constants.PhysicsTimeStep);
            }

        }

        public static Vector3 CalculateAcceleration (Vector3 point, CBody ignoreBody = null) {
            Vector3 acceleration = Vector3.zero;
            foreach (var body in Instance.bodies) {
                if (body != ignoreBody) {
                    float sqrDst = (body.position - point).sqrMagnitude;
                    Vector3 forceDir = (body.position - point).normalized;
                    acceleration += forceDir * Constants.GravitationalConstant * body.mass / sqrDst;
                }
            }

            return acceleration;
        }

        public static CBody[] Bodies {
            get {
                return Instance.bodies;
            }
        }

        static NBodySimulation Instance {
            get {
                if (instance == null) {
                    instance = FindObjectOfType<NBodySimulation> ();
                }
                return instance;
            }
        }
    }
}