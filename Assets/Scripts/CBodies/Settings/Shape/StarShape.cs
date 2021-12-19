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
        [SerializeReference] public StarShapeSettings shapeSettings;

        // MEMENTO PATTERN
        public override void InitSettings()
        {
            shapeSettings = new StarShapeSettings
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
            shapeSettings = (StarShapeSettings) ss;
            if(Observer)
                Observer.OnShapeUpdate();
        }
        // END MEMENTO


        [Serializable]
        public class StarShapeSettings : ShapeSettings
        {
            public float starCazzo;
        }
    }
}