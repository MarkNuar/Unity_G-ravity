using UnityEngine;

namespace CBodies.Noise
{
    public interface INoiseFilter
    {
        public float Evaluate(Vector3 point);
    }
}