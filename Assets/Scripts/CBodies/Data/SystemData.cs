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
            CBodyData c = new CBodyData
            {
                appearance = new AppearanceData(),
                physics = new PhysicsData(),
            };
            c.appearance.Init();
            Vector3 pos = new Vector3(0 - cBodies.Count * 25, 0, 0);
            c.physics.Init(pos);
            c.Init();
            cBodies.Add(c);
            return cBodies.Count - 1;
        }

        // public CBodyData GetCBodyAtIndex(int index)
        // {
        //     return cBodies[index];
        // }

        // public List<CBodyData> GetCBodies()
        // {
        //     return cBodies;
        // }

        // public void RemoveCBodyAtIndex(int index)
        // {
        //     cBodies.RemoveAt(index);
        //     for (var i = index; i < cBodies.Count; i++)
        //     {
        //         cBodies[i].index--;
        //     }
        // }
    }
}
