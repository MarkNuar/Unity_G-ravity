using CBodies.Data;
using UnityEngine;

namespace CBodies
{
    public static class CBodyDispatcher
    {
        // todo
        // this method will take planet shape and physics settings
        public static void GeneratePlanet(CBodyData cBodyData)
        {
            GameObject planet = new GameObject
            {
                transform =
                {
                    position = cBodyData.physics.initialPosition
                }
            };
            planet.AddComponent<Rigidbody>().isKinematic = true;
            planet.AddComponent<CBody>();
            planet.AddComponent<MeshGenerator>();
            // register to the event of cBodyData OnDataChanged.
            // when event fired, update cBody
            // on the cbody, have a method that updated the cbody, call that when OnDataChanged
        }
    }
}
