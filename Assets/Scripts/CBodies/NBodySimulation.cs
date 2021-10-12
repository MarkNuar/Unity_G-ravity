using System.Collections;
using System.Collections.Generic;
using Planets;
using UnityEngine;

public class NBodySimulation : MonoBehaviour {
    CBody[] bodies;
    static NBodySimulation instance;
    
    void Awake () {

        bodies = FindObjectsOfType<CBody> ();
        Time.fixedDeltaTime = Constants.PhysicsTimeStep;
        Debug.Log ("Setting fixedDeltaTime to: " + Constants.PhysicsTimeStep);
    }

    void FixedUpdate () {
        for (int i = 0; i < bodies.Length; i++) {
            Vector3 acceleration = CalculateAcceleration (bodies[i].Position, bodies[i]);
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
                float sqrDst = (body.Position - point).sqrMagnitude;
                Vector3 forceDir = (body.Position - point).normalized;
                acceleration += forceDir * Constants.GravitationalConstant * body.Mass / sqrDst;
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