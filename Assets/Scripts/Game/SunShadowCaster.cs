using UnityEngine;

namespace Game
{
    [RequireComponent(typeof(Light))]
    public class SunShadowCaster : MonoBehaviour {
        public Transform cameraToTrack;
        public bool trackCamera = true;
        
        void LateUpdate () {
            if (trackCamera && cameraToTrack != null) {
                transform.LookAt (-cameraToTrack.position);
            }
        }
    }
}