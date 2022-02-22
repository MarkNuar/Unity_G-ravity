using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace CBodies.Settings
{
    [Serializable]
    public class SystemSettings
    {
        // ***** SERIALIZED
        public string systemName;
        public List<CBodySettings> cBodiesSettings = new List<CBodySettings>();

        public Vector3 lastCameraPosition;
        public float lastCameraZoom;
        // ***** END
        
        
        
        public int AddNewCBody(CBodySettings.CBodyType type)
        {
            CBodySettings cb = new CBodySettings();
            // Initialize cBodySettings values
            cb.Init(type);

            // todo: position according to the type of planet
            var posX = cBodiesSettings.Select(cbs => 
                cbs.physics.GetSettings().initialPosition.x + 
                cbs.physics.GetSettings().maxRadius + 
                cb.physics.GetSettings().maxRadius + 100).Prepend(0.0f).Max();
            Vector3 pos = new Vector3( posX, 0, 0);
            Physics.Physics.PhysicsSettings ps = cb.physics.GetSettings();
            ps.initialPosition = pos;
            cb.physics.SetSettings(ps);

            cBodiesSettings.Add(cb);
            return cBodiesSettings.Count - 1;
        }
    }
}
