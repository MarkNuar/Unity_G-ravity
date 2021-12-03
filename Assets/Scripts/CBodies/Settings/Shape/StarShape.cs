﻿using System;
using Newtonsoft.Json;
using UI.Menu.SystemEditing;
using UnityEngine;
using Random = UnityEngine.Random;

namespace CBodies.Settings.Shape
{
    [Serializable][CreateAssetMenu]
    public class StarShape : Shape
    {

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
    }
}