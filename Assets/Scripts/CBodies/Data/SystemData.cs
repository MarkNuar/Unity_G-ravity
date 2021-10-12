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
        [SerializeField] private List<CBodyData> cBodies = new List<CBodyData>();

        public int AddNewCBody()
        {
            var c = new CBodyData
            {
                appearance = new AppearanceData(),
                physics = new PhysicsData(),
                index = cBodies.Count
            };
            c.appearance.Init();
            c.physics.Init();
            c.Init();
            cBodies.Add(c);
            return c.index;
        }

        public CBodyData GetCBodyAtIndex(int index)
        {
            return cBodies[index];
        }

        public List<CBodyData> GetCBodies()
        {
            return cBodies;
        }

        public void RemoveCBodyAtIndex(int index)
        {
            cBodies.RemoveAt(index);
            for (var i = index; i < cBodies.Count; i++)
            {
                cBodies[i].index--;
            }
        }
    }
}
