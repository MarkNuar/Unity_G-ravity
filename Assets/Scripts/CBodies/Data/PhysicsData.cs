using System;
using UnityEngine;
using Random = UnityEngine.Random;

namespace CBodies.Data
{
    [Serializable]
    public class PhysicsData
    {
        public float mass;
        public float surfaceGravity;
        public Vector3 initialPosition; 
        public Vector3 initialVelocity;

        public void Init(Vector3 pos)
        {
            mass = Random.Range(0, 1000); // TODO TEST THE MASS!
            surfaceGravity = Random.Range(5, 20);
            initialPosition = pos;
            initialVelocity = Vector3.up; // TODO MULTIPLY BY INITIAL VELOCITY RANDOM SCALE
            
        }
    }
}
