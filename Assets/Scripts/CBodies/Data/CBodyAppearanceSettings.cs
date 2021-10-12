using System;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

namespace CBodies.Data
{
    [Serializable]
    public class CBodyAppearanceSettings
    {
        private static readonly string[] BaseNames = {"Plutu","Merci","Nanastria","Regemonia"};
 
        public string Name;
        public Color Color;

        public void Init()
        {
            Name = BaseNames[Random.Range(0, BaseNames.Length)];
            Color = Random.ColorHSV();
            // ...
        }
    }
}
