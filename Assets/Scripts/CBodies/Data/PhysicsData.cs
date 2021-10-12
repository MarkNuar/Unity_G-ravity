using System;
using UnityEngine;
using Random = UnityEngine.Random;

namespace CBodies.Data
{
    [Serializable]
    public class PhysicsData
    {
        public float mass;
        public Vector3 initialVelocity;

        public void Init()
        {
            mass = Random.Range(0, 1000); // TODO TEST THE MASS!
            initialVelocity = Vector3.up; // TODO MULTIPLY BY INITIAL VELOCITY RANDOM SCALE
        }
    }
}
