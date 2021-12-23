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
            if(Observer)
                Observer.OnShapeUpdate();
        }
        public override ShapeSettings GetSettings()
        {
            return shapeSettings;
        }
        public override void SetSettings (ShapeSettings ss)
        {
            shapeSettings = (GaseousShapeSettings)ss;
            if(Observer)
                Observer.OnShapeUpdate();
        }
        // END MEMENTO


        [Serializable]
        public class GaseousShapeSettings : ShapeSettings
        {
            
        }
    }
}