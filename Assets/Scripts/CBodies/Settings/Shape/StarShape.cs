using System;
using Newtonsoft.Json;
using UI.Menu.SystemEditing;
using UnityEngine;
using Random = UnityEngine.Random;

namespace CBodies.Settings.Shape
{
    [Serializable][CreateAssetMenu]
    public class StarShape : Shape
    {
        // MEMENTO
        [SerializeReference] protected ShapeSettings shapeSettings;

        public override void RandomInitialize(int res)
        {
            shapeSettings = new StarShapeSettings
            {
                resolution = res,
                mountainsHeight = Random.Range(ParameterValues.minMountainsHeight, ParameterValues.maxMountainsHeight),
                perturbStrength = 0.6f,
                starCazzo = 0.1f
                // ...
            };
            if(Observer)
                Observer.OnShapeUpdate();
        }


        [Serializable]
        public class StarShapeSettings : ShapeSettings
        {
            public float starCazzo;
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