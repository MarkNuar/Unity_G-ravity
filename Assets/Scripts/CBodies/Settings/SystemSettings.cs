using System;
using System.Collections.Generic;
using UnityEngine;

namespace CBodies.Settings
{
    [Serializable]
    public class SystemSettings
    {
        public string systemName;
        public List<CBodySettings> cBodiesSettings = new List<CBodySettings>();

        public int AddNewCBody(CBodySettings.CBodyType type)
        {
            CBodySettings cbd = new CBodySettings();
            cbd.Init(type);
            
            // todo: position according to the type of planet
            var count = cBodiesSettings.Count;
            Vector3 pos = new Vector3( count * 25, 0, 0);
            cbd.physics.Init(pos);
            
            // todo: update resolution in the editing menu according to camera distance from planets
            var res = 10;
            cbd.shape.Init(res);
            
            cbd.shading.Init();

            
            
            cBodiesSettings.Add(cbd);
            return cBodiesSettings.Count - 1;
        }
    }
}
