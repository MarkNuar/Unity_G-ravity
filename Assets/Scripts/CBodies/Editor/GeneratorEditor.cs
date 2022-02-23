using CBodies.Settings;
using CBodies.Settings.PostProcessing.Atmosphere;
using CBodies.Settings.PostProcessing.Ocean;
using CBodies.Settings.PostProcessing.Ring;
using CBodies.Settings.Shading;
using CBodies.Settings.Shape;
using UnityEditor;
using UnityEngine;
using Physics = CBodies.Settings.Physics.Physics;

namespace CBodies.Editor
{
    [CustomEditor(typeof (CBodyGenerator))]
    public class GeneratorEditor : UnityEditor.Editor
    {
        private CBodyGenerator _cBodyGenerator;
        private UnityEditor.Editor _shapeEditor;
        private UnityEditor.Editor _shadingEditor;
        private UnityEditor.Editor _physicsEditor;
        private UnityEditor.Editor _oceanEditor;
        private UnityEditor.Editor _atmosphereEditor;
        private UnityEditor.Editor _ringEditor;

        private bool _shapeFoldout;
        private bool _shadingFoldout;
        private bool _physicsFoldout;
        private bool _oceanFoldout;
        private bool _atmosphereFoldout;
        private bool _ringFoldout;

        public override void OnInspectorGUI()
        {
            Shading.ShadingSettings shadingS = _cBodyGenerator.cBodySettings.shading.GetSettings();
            Shape.ShapeSettings shapeS = _cBodyGenerator.cBodySettings.shape.GetSettings();
            Physics.PhysicsSettings physicsS = _cBodyGenerator.cBodySettings.physics.GetSettings();
            Ocean.OceanSettings oceanS = _cBodyGenerator.cBodySettings.ocean.GetSettings();
            Atmosphere.AtmosphereSettings atmosphereS = _cBodyGenerator.cBodySettings.atmosphere.GetSettings();
            Ring.RingSettings ringS = _cBodyGenerator.cBodySettings.ring.GetSettings();

            CBodySettings.CBodyType newValue =
                (CBodySettings.CBodyType) EditorGUILayout.EnumPopup(_cBodyGenerator.cBodySettings.cBodyType);

            if (newValue != _cBodyGenerator.cBodySettings.cBodyType)
            {
                _cBodyGenerator.cBodySettings.UpdateCBodyType(newValue);
                shadingS = _cBodyGenerator.cBodySettings.shading.GetSettings();
                shapeS = _cBodyGenerator.cBodySettings.shape.GetSettings();
                physicsS = _cBodyGenerator.cBodySettings.physics.GetSettings();
                oceanS = _cBodyGenerator.cBodySettings.ocean.GetSettings();
                atmosphereS = _cBodyGenerator.cBodySettings.atmosphere.GetSettings();
                ringS = _cBodyGenerator.cBodySettings.ring.GetSettings();
                // link the new settings to the generator
                _cBodyGenerator.cBodySettings.Subscribe(_cBodyGenerator);
                Regenerate(shapeS,shadingS,physicsS, oceanS, atmosphereS, ringS);
            }

            using (var check = new EditorGUI.ChangeCheckScope ()) {
                DrawDefaultInspector ();
                DrawSettings();
                if (check.changed) {
                    Regenerate (shapeS, shadingS, physicsS, oceanS, atmosphereS, ringS);
                }
            }

            if (GUILayout.Button ("Randomize Shading")) {
                shadingS.RandomizeShading(true);
                oceanS.RandomizeShading(true);
                atmosphereS.RandomizeShading(true);
                ringS.RandomizeShading(true);
                Regenerate (shapeS, shadingS, physicsS, oceanS, atmosphereS, ringS);
            }

            if (GUILayout.Button ("Randomize Shape")) {
                shapeS.RandomizeShape(true);
                oceanS.RandomizeShape(true);
                ringS.RandomizeShape(true);
                Regenerate (shapeS, shadingS, physicsS, oceanS, atmosphereS, ringS);
            }

            if (GUILayout.Button ("Randomize All")) {
                shadingS.RandomizeShading(true);
                shapeS.RandomizeShape(true);
                oceanS.RandomizeShading(true);
                oceanS.RandomizeShape(true);
                atmosphereS.RandomizeShading(true);
                ringS.RandomizeShading(true);
                ringS.RandomizeShape(true);
                Regenerate (shapeS, shadingS, physicsS, oceanS, atmosphereS, ringS);
            }

            var randomized = shadingS.randomize || shapeS.randomize || oceanS.randomizeShading || oceanS.randomizeHeight || ringS.randomizeShading || atmosphereS.randomizeShading ;
            randomized |= shadingS.seed != 0 || shapeS.seed != 0 || oceanS.shadingSeed != 0 || oceanS.heightSeed != 0 || ringS.shadingSeed != 0 || atmosphereS.shadingSeed != 0;
            using (new EditorGUI.DisabledGroupScope (!randomized)) {
                if (GUILayout.Button ("Reset Randomization")) {
                    shadingS.RandomizeShading(false);
                    shapeS.RandomizeShape(false);
                    oceanS.RandomizeShading(false);
                    oceanS.RandomizeShape(false);
                    atmosphereS.RandomizeShading(false);
                    ringS.RandomizeShading(false);
                    ringS.RandomizeShape(false);
                    Regenerate (shapeS, shadingS, physicsS, oceanS, atmosphereS, ringS);
                }
                
                var realistic = shadingS.realisticColors && oceanS.realisticColors && atmosphereS.realisticColors;
                using (new EditorGUI.DisabledGroupScope(!realistic))
                {
                    if (GUILayout.Button("Non Realistic Colors"))
                    {
                        shadingS.realisticColors = false;
                        oceanS.realisticColors = false;
                        atmosphereS.realisticColors = false;
                        Regenerate (shapeS, shadingS, physicsS, oceanS, atmosphereS, ringS);
                    }
                }
                using (new EditorGUI.DisabledGroupScope(realistic))
                {
                    if (GUILayout.Button("Realistic Colors"))
                    {
                        shadingS.realisticColors = true;
                        oceanS.realisticColors = true;
                        atmosphereS.realisticColors = true;
                        Regenerate (shapeS, shadingS, physicsS, oceanS, atmosphereS, ringS);
                    }
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
            DrawSettingsEditor(_cBodyGenerator.cBodySettings.atmosphere, ref _atmosphereFoldout, ref _atmosphereEditor);
            DrawSettingsEditor (_cBodyGenerator.cBodySettings.ring, ref _ringFoldout, ref _ringEditor);
        }
        
        
        private void Regenerate (Shape.ShapeSettings sp, Shading.ShadingSettings sd, Physics.PhysicsSettings ps, 
            Ocean.OceanSettings os, Atmosphere.AtmosphereSettings aa, Ring.RingSettings rs) 
        {
            // The order matters! 
            // Set the ocean update before shading update 
            // Otherwise the shading shader will get a wrong ocean level value
            _cBodyGenerator.cBodySettings.atmosphere.SetSettings(aa);
            _cBodyGenerator.cBodySettings.ocean.SetSettings(os);
            _cBodyGenerator.cBodySettings.ring.SetSettings(rs);
            _cBodyGenerator.cBodySettings.shape.SetSettings(sp);
            _cBodyGenerator.cBodySettings.shading.SetSettings(sd);
            _cBodyGenerator.cBodySettings.physics.SetSettings(ps);
            EditorApplication.QueuePlayerLoopUpdate ();
        }

        private void DrawSettingsEditor (Object settings, ref bool foldout, ref UnityEditor.Editor editor) {
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
            _atmosphereFoldout = EditorPrefs.GetBool(nameof(_atmosphereFoldout), false);
            _ringFoldout = EditorPrefs.GetBool(nameof(_ringFoldout), false);
            _cBodyGenerator = (CBodyGenerator) target;
        }

        private void SaveState () {
            EditorPrefs.SetBool(nameof (_shapeFoldout), _shapeFoldout);
            EditorPrefs.SetBool(nameof (_shadingFoldout), _shadingFoldout);
            EditorPrefs.SetBool(nameof (_physicsFoldout), _physicsFoldout);
            EditorPrefs.SetBool(nameof (_oceanFoldout), _oceanFoldout);
            EditorPrefs.SetBool(nameof (_atmosphereFoldout), _atmosphereFoldout);
            EditorPrefs.SetBool(nameof (_ringFoldout), _ringFoldout);
        }
    }
}