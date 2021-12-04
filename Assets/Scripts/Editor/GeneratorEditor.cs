using CBodies;
using CBodies.Settings;
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
        CBodyGenerator generator;
        UnityEditor.Editor shapeEditor;
        UnityEditor.Editor shadingEditor;
        UnityEditor.Editor physicsEditor;

        bool shapeFoldout;
        bool shadingFoldout;
        bool physicsFoldout;

        public override void OnInspectorGUI()
        {
            Shading.ShadingSettings sd = generator.cBodySettings.shading.GetSettings();
            Shape.ShapeSettings sp = generator.cBodySettings.shape.GetSettings();
            Physics.PhysicsSettings ps = generator.cBodySettings.physics.GetSettings();

            using (var check = new EditorGUI.ChangeCheckScope ()) {
                DrawDefaultInspector ();
                DrawSettings();
                if (check.changed) {
                    Regenerate (sp, sd, ps);
                }
            }

            if (GUILayout.Button ("Generate")) {
                Regenerate (sp, sd, ps);
            }
            if (GUILayout.Button ("Randomize Shading")) {
                var prng = new System.Random ();
                sd.randomize = true;
                sd.seed = prng.Next (-10000, 10000);
                Regenerate (sp, sd, ps);
            }

            if (GUILayout.Button ("Randomize Shape")) {
                var prng = new System.Random ();
                sp.randomize = true;
                sp.seed = prng.Next (-10000, 10000);
                generator.cBodySettings.shape.SetSettings(sp);
                Regenerate (sp, sd, ps);
            }

            if (GUILayout.Button ("Randomize Both")) {
                var prng = new System.Random ();
                sd.randomize = true;
                sp.randomize = true;
                sp.seed = prng.Next (-10000, 10000);
                sd.seed = prng.Next (-10000, 10000);
                generator.cBodySettings.shape.SetSettings(sp);
                generator.cBodySettings.shading.SetSettings(sd);
                Regenerate (sp, sd, ps);
            }

            bool randomized = sd.randomize || sp.randomize;
            randomized |= sd.seed != 0 || sp.seed != 0;
            using (new EditorGUI.DisabledGroupScope (!randomized)) {
                if (GUILayout.Button ("Reset Randomization")) {
                    var prng = new System.Random ();
                    sd.randomize = false;
                    sp.randomize = false;
                    sp.seed = 0;
                    sd.seed = 0;
                    generator.cBodySettings.shape.SetSettings(sp);
                    generator.cBodySettings.shading.SetSettings(sd);
                    Regenerate (sp, sd, ps);
                }
            }

            SaveState ();
        }

        private void DrawSettings()
        {
            // Draw shape/shading object editors
            DrawSettingsEditor (generator.cBodySettings.shape, ref shapeFoldout, ref shapeEditor);
            DrawSettingsEditor (generator.cBodySettings.shading, ref shadingFoldout, ref shadingEditor);
            DrawSettingsEditor (generator.cBodySettings.physics, ref physicsFoldout, ref physicsEditor);
        }
        
        
        void Regenerate (Shape.ShapeSettings sp, Shading.ShadingSettings sd, Physics.PhysicsSettings ps) {
            generator.cBodySettings.shape.SetSettings(sp);
            generator.cBodySettings.shading.SetSettings(sd);
            generator.cBodySettings.physics.SetSettings(ps);
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
            shapeFoldout = EditorPrefs.GetBool (nameof (shapeFoldout), false);
            shadingFoldout = EditorPrefs.GetBool (nameof (shadingFoldout), false);
            physicsFoldout = EditorPrefs.GetBool (nameof (physicsFoldout), false);
            generator = (CBodyGenerator) target;
        }

        void SaveState () {
            EditorPrefs.SetBool (nameof (shapeFoldout), shapeFoldout);
            EditorPrefs.SetBool (nameof (shadingFoldout), shadingFoldout);
            EditorPrefs.SetBool (nameof (physicsFoldout), physicsFoldout);
        }
    }
}