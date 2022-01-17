using UnityEngine;

namespace Game
{
    [RequireComponent(typeof(Light))]
    public class SunShadowCaster : MonoBehaviour {
        public Transform cameraToTrack;

        // void LateUpdate () {
        //     if (cameraToTrack) {
        //         transform.LookAt (-cameraToTrack.position);
        //     }
        // }
    }
}