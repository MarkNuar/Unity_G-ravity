using System;
using System.Collections.Generic;
using UnityEngine;

namespace CBodies.Settings
{
    [Serializable]
    public class SystemSettings
    {
        // ***** SERIALIZED
        public string systemName;
        public List<CBodySettings> cBodiesSettings = new List<CBodySettings>();
        // ***** END
        
        
        
        public int AddNewCBody(CBodySettings.CBodyType type)
        {
            CBodySettings cbd = new CBodySettings();
            // Initialize cBodySettings values
            cbd.Init(type);
            cbd.shape.InitSettings();
            cbd.shading.InitSettings();
            cbd.physics.InitSettings();
            
            // todo: position according to the type of planet
            var count = cBodiesSettings.Count;
            Vector3 pos = new Vector3( count * 25, 0, 0);
            Physics.Physics.PhysicsSettings ps = cbd.physics.GetSettings();
            ps.initialPosition = pos;
            cbd.physics.SetSettings(ps);

            cBodiesSettings.Add(cbd);
            return cBodiesSettings.Count - 1;
        }
    }
}
