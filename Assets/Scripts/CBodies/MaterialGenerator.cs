using CBodies.Data;
using CBodies.Noise;
using UnityEngine;

namespace CBodies
{
    public class MaterialGenerator
    {
        private MaterialData _materialData;
        private INoiseFilter[] _noiseFilters;

        public void UpdateData(CBodyData cbd)
        {
            _materialData = cbd.materialData;
            _noiseFilters = DataToNoise.Convert(cbd.materialData, cbd.cBodyType);
        }
    }
}
