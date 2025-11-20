#if UNITY_EDITOR
using System;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace AbyssMoth
{
    [InitializeOnLoad]
    public static class RequiredValidationRunner
    {
        private static double lastTick;

        static RequiredValidationRunner()
        {
            EditorApplication.playModeStateChanged += OnPlayModeStateChanged;
            EditorSceneManager.sceneOpened += OnSceneOpened;
            EditorApplication.update += OnUpdate;
        }

        [MenuItem("RimuruDev Tools/EditorValidator/Validate Now")]
        public static void ValidateNow()
        {
            var s = RequiredValidationSettings.Load();
            var scope = BuildScope(s);
            RequiredReferenceValidator.Validate(scope, s ?? new RequiredValidationSettings());
        }

        [MenuItem("Assets/Create/EditorValidator/Required Validation Settings")]
        public static void CreateSettings()
        {
            var asset = ScriptableObject.CreateInstance<RequiredValidationSettings>();
            var dir = "Assets/Resources";
            if (!AssetDatabase.IsValidFolder("Assets/Resources")) AssetDatabase.CreateFolder("Assets", "Resources");
            var path = AssetDatabase.GenerateUniqueAssetPath(dir + "/RequiredValidationSettings.asset");
            AssetDatabase.CreateAsset(asset, path);
            AssetDatabase.SaveAssets();
            EditorUtility.FocusProjectWindow();
            Selection.activeObject = asset;
        }

        private static void OnSceneOpened(UnityEngine.SceneManagement.Scene scene, OpenSceneMode mode)
        {
            var s = RequiredValidationSettings.Load();
            if (s != null && s.ValidateOnSceneOpen)
            {
                var scope = BuildScopeForSceneOpen(s);
                RequiredReferenceValidator.Validate(scope, s);
            }
        }

        private static void OnPlayModeStateChanged(PlayModeStateChange change)
        {
            if (change != PlayModeStateChange.ExitingEditMode)
                return;
            var s = RequiredValidationSettings.Load();
            if (s == null || !s.ValidateOnPlay)
                return;
            var scope = BuildScope(s);
            var issues = RequiredReferenceValidator.Validate(scope, s);
            if (issues.Count > 0 && s.CancelPlayOnError)
            {
                EditorApplication.isPlaying = false;
                EditorUtility.DisplayDialog("Validation", "Play cancelled. Issues: " + issues.Count, "OK");
            }
        }

        private static void OnUpdate()
        {
            // TOOD: заблокал вализацию пока в игре
            if(Application.isPlaying == true)
                return;
            
            var s = RequiredValidationSettings.Load();
            if (s == null || !s.ValidateByTimer) return;
            var now = EditorApplication.timeSinceStartup;
            if (now - lastTick < Math.Max(1f, s.TimerSeconds)) return;
            lastTick = now;
            var scope = BuildScopeForTimer(s);
            RequiredReferenceValidator.Validate(scope, s);
        }

        private static ValidationScope BuildScope(RequiredValidationSettings s)
        {
            var scope = ValidationScope.None;
            if (s == null || s.ScanOpenScenes) scope |= ValidationScope.OpenScenes;
            if (s != null && s.ScanPrefabs) scope |= ValidationScope.Prefabs;
            if (s != null && s.ScanScriptableObjects) scope |= ValidationScope.ScriptableObjects;
            return scope;
        }

        private static ValidationScope BuildScopeForSceneOpen(RequiredValidationSettings s)
        {
            var scope = ValidationScope.None;
            if (s.ScanOpenScenes) scope |= ValidationScope.OpenScenes;
            return scope;
        }

        private static ValidationScope BuildScopeForTimer(RequiredValidationSettings s)
        {
            var scope = ValidationScope.None;
            if (s.ScanOpenScenes) scope |= ValidationScope.OpenScenes;
            return scope;
        }
    }
}
#endif