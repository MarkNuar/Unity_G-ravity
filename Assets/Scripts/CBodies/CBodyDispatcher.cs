using UnityEngine;
using UnityEngine.UI;

namespace Planets
{
    public static class CBodyDispatcher
    {
        // todo
        // this method will take planet shape and physics settings
        public static void GeneratePlanet()
        {
            var planet = new GameObject();
            planet.AddComponent<Rigidbody>().isKinematic = true;
            planet.AddComponent<CBody>();
            planet.AddComponent<MeshGenerator>();
        }
    }
}
