using System;
using UnityEngine;
using Random = UnityEngine.Random;

namespace CBodies.Settings.Shading
{
    [Serializable][CreateAssetMenu]
    public class RockShading : Shading
    {
        // MEMENTO
        [SerializeReference] protected ShadingSettings shadingSettings;
        
        public override void RandomInitialize()
        {
            shadingSettings = new RockShadingSettings
            {
                color = Random.ColorHSV()
                // ...
            };
            if(Observer)
                Observer.OnShadingUpdate();
        }
        
        [Serializable]
        public class RockShadingSettings : ShadingSettings
        {
            
        }

        // MEMENTO PATTERN
        public override ShadingSettings GetSettings()
        {
            return shadingSettings;
        }

        public override void SetSettings(ShadingSettings ss)
        {
            shadingSettings = ss;
            if(Observer)
                Observer.OnShadingUpdate();
        }
        
        // VISITOR PATTERN
        public override void AcceptVisitor(ISettingsVisitor visitor)
        {
            visitor.VisitShadingSettings(this);
        }
    }
}