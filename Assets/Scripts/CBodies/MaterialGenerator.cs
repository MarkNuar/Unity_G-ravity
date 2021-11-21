using CBodies.Data;
using CBodies.Noise;
using UnityEngine;

namespace CBodies
{
    public class MaterialGenerator
    {
        private ShadingData _shadingData;
        private INoiseFilter[] _noiseFilters;

        public void UpdateData(CBodyData cbd)
        {
            _shadingData = cbd.shadingData;
            _noiseFilters = DataToNoise.Convert(cbd.shadingData, cbd.cBodyType);
        }
    }
}
