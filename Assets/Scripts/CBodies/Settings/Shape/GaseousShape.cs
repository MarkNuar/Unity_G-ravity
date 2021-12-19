using System;
using UI.Menu.SystemEditing;
using UnityEngine;
using Random = UnityEngine.Random;

namespace CBodies.Settings.Shape
{
    [Serializable][CreateAssetMenu]
    public class GaseousShape : Shape
    {
        // MEMENTO
        [SerializeReference] public GaseousShapeSettings shapeSettings;

        
        // MEMENTO PATTERN
        public override void InitSettings()
        {
            shapeSettings = new GaseousShapeSettings
            {
                perturbVertices = false
            };
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