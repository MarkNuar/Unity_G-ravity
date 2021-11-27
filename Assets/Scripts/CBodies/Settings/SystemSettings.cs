using System;
using System.Collections.Generic;
using CBodies.Settings.Shading;
using CBodies.Settings.Shape;
using UnityEngine;
using Physics = CBodies.Settings.Physics.Physics;

namespace CBodies.CBodySettings
{
    [Serializable]
    public class SystemSettings
    {
        public string systemName;
        public List<CBodies.CBodySettings.CBodySettings> cBodies = new List<CBodies.CBodySettings.CBodySettings>();

        public int AddNewCBody()
        {
            CBodies.CBodySettings.CBodySettings cbd = new CBodies.CBodySettings.CBodySettings
            {
                shape = new Shape(),
                shading = new Shading(),
                physics = new Physics(),
            };
            
            // todo: position according to the type of planet
            var count = cBodies.Count;
            Vector3 pos = new Vector3( count * 25, 0, 0);
            cbd.physics.Init(pos);
            
            // todo: update resolution in the editing menu according to camera distance from planets
            var res = 10;
            cbd.shape.Init(res);
            
            cbd.shading.Init();

            cbd.Init();
            
            cBodies.Add(cbd);
            return cBodies.Count - 1;
        }
    }
}
