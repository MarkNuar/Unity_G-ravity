using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

namespace CBodies.Data
{
    [Serializable]
    public class SystemData
    {
        public string systemName;
        public List<CBodyData> cBodies = new List<CBodyData>();

        public int AddNewCBody()
        {
            CBodyData cbd = new CBodyData
            {
                meshData = new MeshData(),
                materialData = new MaterialData(),
                physicsData = new PhysicsData(),
            };
            
            // todo: position according to the type of planet
            var count = cBodies.Count;
            Vector3 pos = new Vector3( count * 25, 0, 0);
            cbd.physicsData.Init(pos);
            
            // todo: update resolution in the editing menu according to camera distance from planets
            var res = 10;
            cbd.meshData.Init(res);
            
            cbd.materialData.Init();

            cbd.Init();
            
            cBodies.Add(cbd);
            return cBodies.Count - 1;
        }
    }
}
