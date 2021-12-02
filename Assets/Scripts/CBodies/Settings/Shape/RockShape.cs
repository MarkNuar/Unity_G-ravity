using System;
using UI.Menu.SystemEditing;
using UnityEngine;
using Random = UnityEngine.Random;

namespace CBodies.Settings.Shape
{
    [Serializable][CreateAssetMenu]
    public class RockShape : Shape
    {

        public override void RandomInitialize(int res)
        {
            base.RandomInitialize(res);
            shapeSettings = new RockShapeSettings
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
        public class RockShapeSettings : ShapeSettings
        {
            
        }
    }
}