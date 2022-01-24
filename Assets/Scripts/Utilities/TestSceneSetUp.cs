using CBodies;
using CBodies.Settings;
using UnityEngine;
using Physics = CBodies.Settings.Physics.Physics;

namespace Utilities
{
    public class TestSceneSetUp : MonoBehaviour
    {
        public CBody cBody;
        public CBodySettings.CBodyType type;
    
        private SystemSettings _ss;
        private CBodySettings _cs;
    
        // Start is called before the first frame update
        void Start()
        {
            _ss = SystemUtils.Instance.LoadTestSystem("Test");
            // If system not loaded or loaded cBody has different type from the one set in the inspector
            // create a new system
            if (_ss == null || _ss.cBodiesSettings[0].cBodyType != type)
            {
                _ss = new SystemSettings
                {
                    systemName = "Test"
                };
                _ss.AddNewCBody(type);
            }
            
            
            _cs = _ss.cBodiesSettings[0];
            _cs.Subscribe(cBody.cBodyGenerator);

            Physics.PhysicsSettings ps = _cs.physics.GetSettings();
            ps.rotationSpeed = 0;
            _cs.physics.SetSettings(ps);

            // Set current system settings, available to whole program
            SystemUtils.Instance.currentSystemSettings = _ss;
        }

        private void OnDestroy()
        {
            SystemUtils.Instance.SaveTestSystem(_ss);
        }
    }
}
