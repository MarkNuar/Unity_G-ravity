using System;
using UnityEngine;

namespace CBodies.Settings.Shape
{
    [Serializable][CreateAssetMenu]
    public class GaseousShape : Shape
    {
        // MEMENTO
        [SerializeField] public GaseousShapeSettings shapeSettings;

        
        // MEMENTO PATTERN
        public override void InitSettings()
        {
            if (Observers == null) return;
            foreach (ICBodyObserver o in Observers)
            {
                o.OnShapeUpdate();
            }
        }
        public override ShapeSettings GetSettings()
        {
            return shapeSettings;
        }
        public override void SetSettings (ShapeSettings ss)
        {
            shapeSettings = (GaseousShapeSettings)ss;
            
            if (Observers == null) return;
            foreach (ICBodyObserver o in Observers)
            {
                o.OnShapeUpdate();
            }
        }
        // END MEMENTO


        [Serializable]
        public class GaseousShapeSettings : ShapeSettings
        {
            
        }
    }
}