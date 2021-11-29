using System;
using System.Collections.Generic;
using UnityEngine;

namespace CBodies.Settings
{
    [Serializable]
    public class SystemSettings
    {
        public string systemName;
        public List<CBodySettings> cBodiesSettings;

        public int AddNewCBody()
        {
            CBodySettings cbd = new CBodySettings
            {
                // todo: load the correct cbody type from the serializer
                shape = new Shape.Shape(),
                shading = new Shading.Shading(),
                physics = new Physics.Physics(),
            };
            
            // todo: position according to the type of planet
            var count = cBodiesSettings.Count;
            Vector3 pos = new Vector3( count * 25, 0, 0);
            cbd.physics.Init(pos);
            
            // todo: update resolution in the editing menu according to camera distance from planets
            var res = 10;
            cbd.shape.Init(res);
            
            cbd.shading.Init();

            cbd.Init();
            
            cBodiesSettings.Add(cbd);
            return cBodiesSettings.Count - 1;
        }
    }
}
