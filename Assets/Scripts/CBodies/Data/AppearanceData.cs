using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;
using Random = UnityEngine.Random;

namespace CBodies.Data
{
    [Serializable]
    public class AppearanceData
    {
        public Color color;

        public void Init()
        {
            color = Random.ColorHSV();
            // ...
        }
    }
}
