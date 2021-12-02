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
            // // Subscribe the cBody to the newly created cBody settings 
            // cbd.Subscribe(observer);

            // todo: position according to the type of planet
            var count = cBodiesSettings.Count;
            Vector3 pos = new Vector3( count * 25, 0, 0);
            cbd.Physics.RandomInitialize(pos);
            
            // todo: update resolution in the editing menu according to camera distance from planets
            var res = 10;
            cbd.Shape.RandomInitialize(res);
            
            cbd.Shading.RandomInitialize();

            
            
            cBodiesSettings.Add(cbd);
            return cBodiesSettings.Count - 1;
        }
    }
}
