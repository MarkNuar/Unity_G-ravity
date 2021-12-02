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
        [SerializeReference] protected ShapeSettings shapeSettings;

        public override void RandomInitialize(int res)
        {
            shapeSettings = new GaseousShapeSettings()
            {
                resolution = res,
                mountainsHeight = Random.Range(ParameterValues.minMountainsHeight, ParameterValues.maxMountainsHeight),
                perturbStrength = 0.7f
                // ...
            };
            if(Observer)
                Observer.OnShapeUpdate();
        }


        [Serializable]
        public class GaseousShapeSettings : ShapeSettings
        {
            
        }

        // MEMENTO PATTERN
        public override ShapeSettings GetSettings()
        {
            return shapeSettings;
        }

        public override void SetSettings (ShapeSettings ss)
        {
            shapeSettings = ss;
            if(Observer)
                Observer.OnShapeUpdate();
        }
        
                
        // VISITOR PATTERN
        public override void AcceptVisitor(ISettingsVisitor visitor)
        {
            visitor.VisitShapeSettings(this);
        }
    }
}