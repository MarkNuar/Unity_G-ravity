using CBodies;
using CBodies.Settings;
using CBodies.Settings.Ocean;
using CBodies.Settings.Shading;
using CBodies.Settings.Shape;
using UnityEditor;
using UnityEngine;
using Physics = CBodies.Settings.Physics.Physics;

namespace Editor
{
    [CustomEditor(typeof (CBodyGenerator))]
    public class GeneratorEditor : UnityEditor.Editor
    {
        private CBodyGenerator _cBodyGenerator;
        private UnityEditor.Editor _shapeEditor;
        private UnityEditor.Editor _shadingEditor;
        private UnityEditor.Editor _physicsEditor;
        private UnityEditor.Editor _oceanEditor;

        private bool _shapeFoldout;
        private bool _shadingFoldout;
        private bool _physicsFoldout;
        private bool _oceanFoldout;

        public override void OnInspectorGUI()
        {
            Shading.ShadingSettings sd = _cBodyGenerator.cBodySettings.shading.GetSettings();
            Shape.ShapeSettings sp = _cBodyGenerator.cBodySettings.shape.GetSettings();
            Physics.PhysicsSettings ps = _cBodyGenerator.cBodySettings.physics.GetSettings();
            Ocean.OceanSettings os = _cBodyGenerator.cBodySettings.ocean.GetSettings();

            CBodySettings.CBodyType newValue =
                (CBodySettings.CBodyType) EditorGUILayout.EnumPopup(_cBodyGenerator.cBodySettings.cBodyType);

            if (newValue != _cBodyGenerator.cBodySettings.cBodyType)
            {
                _cBodyGenerator.cBodySettings.UpdateCBodyType(newValue);
                sd = _cBodyGenerator.cBodySettings.shading.GetSettings();
                sp = _cBodyGenerator.cBodySettings.shape.GetSettings();
                ps = _cBodyGenerator.cBodySettings.physics.GetSettings();
                // link the new settings to the generator
                _cBodyGenerator.cBodySettings.Subscribe(_cBodyGenerator);
                Regenerate(sp,sd,ps, os);
            }

            using (var check = new EditorGUI.ChangeCheckScope ()) {
                DrawDefaultInspector ();
                DrawSettings();
                if (check.changed) {
                    Regenerate (sp, sd, ps, os);
                }
            }

            if (GUILayout.Button ("Generate")) {
                Regenerate (sp, sd, ps, os);
            }
            if (GUILayout.Button ("Randomize Shading")) {
                var prng = new System.Random ();
                sd.randomize = true;
                sd.seed = prng.Next (-10000, 10000);
                Regenerate (sp, sd, ps, os);
            }

            if (GUILayout.Button ("Randomize Shape")) {
                var prng = new System.Random ();
                sp.randomize = true;
                sp.seed = prng.Next (-10000, 10000);
                _cBodyGenerator.cBodySettings.shape.SetSettings(sp);
                Regenerate (sp, sd, ps, os);
            }
            
            if (GUILayout.Button ("Randomize Ocean")) {
                var prng = new System.Random ();
                os.randomize = true;
                os.seed = prng.Next (-10000, 10000);
                _cBodyGenerator.cBodySettings.ocean.SetSettings(os);
                Regenerate (sp, sd, ps, os);
            }
            
            // TODO ADD RANDOMIZE OCEAN, RANDOMIZE ATMOSPHERE 

            if (GUILayout.Button ("Randomize All")) {
                var prng = new System.Random ();
                sd.randomize = true;
                sp.randomize = true;
                os.randomize = true;
                sp.seed = prng.Next (-10000, 10000);
                sd.seed = prng.Next (-10000, 10000);
                os.seed = prng.Next(-10000, 10000);
                _cBodyGenerator.cBodySettings.shape.SetSettings(sp);
                _cBodyGenerator.cBodySettings.shading.SetSettings(sd);
                _cBodyGenerator.cBodySettings.ocean.SetSettings(os);
                Regenerate (sp, sd, ps, os);
            }

            bool randomized = sd.randomize || sp.randomize || os.randomize;
            randomized |= sd.seed != 0 || sp.seed != 0 || os.seed != 0;
            using (new EditorGUI.DisabledGroupScope (!randomized)) {
                if (GUILayout.Button ("Reset Randomization")) {
                    sd.randomize = false;
                    sp.randomize = false;
                    os.randomize = false;
                    sp.seed = 0;
                    sd.seed = 0;
                    os.seed = 0;
                    _cBodyGenerator.cBodySettings.shape.SetSettings(sp);
                    _cBodyGenerator.cBodySettings.shading.SetSettings(sd);
                    _cBodyGenerator.cBodySettings.ocean.SetSettings(os);
                    Regenerate (sp, sd, ps, os);
                }
            }

            SaveState ();
        }

        private void DrawSettings()
        {
            // Draw shape/shading object editors
            DrawSettingsEditor (_cBodyGenerator.cBodySettings.shape, ref _shapeFoldout, ref _shapeEditor);
            DrawSettingsEditor (_cBodyGenerator.cBodySettings.shading, ref _shadingFoldout, ref _shadingEditor);
            DrawSettingsEditor (_cBodyGenerator.cBodySettings.physics, ref _physicsFoldout, ref _physicsEditor);
            DrawSettingsEditor (_cBodyGenerator.cBodySettings.ocean, ref _oceanFoldout, ref _oceanEditor);
        }
        
        
        void Regenerate (Shape.ShapeSettings sp, Shading.ShadingSettings sd, Physics.PhysicsSettings ps, Ocean.OceanSettings os) 
        {
            _cBodyGenerator.cBodySettings.shape.SetSettings(sp);
            _cBodyGenerator.cBodySettings.shading.SetSettings(sd);
            _cBodyGenerator.cBodySettings.physics.SetSettings(ps);
            _cBodyGenerator.cBodySettings.ocean.SetSettings(os);
            EditorApplication.QueuePlayerLoopUpdate ();
        }

        void DrawSettingsEditor (Object settings, ref bool foldout, ref UnityEditor.Editor editor) {
            if (settings != null) {
                foldout = EditorGUILayout.InspectorTitlebar (foldout, settings);
                if (foldout) {
                    CreateCachedEditor (settings, null, ref editor);
                    editor.OnInspectorGUI ();
                }
            }
        }

        private void OnEnable () {
            _shapeFoldout = EditorPrefs.GetBool (nameof (_shapeFoldout), false);
            _shadingFoldout = EditorPrefs.GetBool (nameof (_shadingFoldout), false);
            _physicsFoldout = EditorPrefs.GetBool (nameof (_physicsFoldout), false);
            _oceanFoldout = EditorPrefs.GetBool (nameof (_oceanFoldout), false);
            _cBodyGenerator = (CBodyGenerator) target;
        }

        void SaveState () {
            EditorPrefs.SetBool (nameof (_shapeFoldout), _shapeFoldout);
            EditorPrefs.SetBool (nameof (_shadingFoldout), _shadingFoldout);
            EditorPrefs.SetBool (nameof (_physicsFoldout), _physicsFoldout);
            EditorPrefs.SetBool (nameof (_oceanFoldout), _oceanFoldout);
        }
    }
}