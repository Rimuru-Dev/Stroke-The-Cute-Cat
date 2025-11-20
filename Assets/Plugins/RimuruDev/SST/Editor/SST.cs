#if UNITY_EDITOR
using System;
using System.IO;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEngine.SceneManagement;
using UnityEditor.SceneManagement;

namespace AbyssMoth
{
    [InitializeOnLoad]
    public static class SST
    {
        // TODO: Provide config
        private const string BootScenePath = "Assets/Internal/Scenes/Boot.unity";
        private static int selectedIndex;
        private static string lastActiveScene = "";
        private static float dropdownBoxHeight = 20f;
        private static bool pendingBootPlay;
        private static VisualElement toolbarUI;
        private static string[] sceneNames = Array.Empty<string>();

        private static bool FetchAllScenes
        {
            get => EditorPrefs.GetBool("SceneSwitcher_FetchAllScenes", false);
            set => EditorPrefs.SetBool("SceneSwitcher_FetchAllScenes", value);
        }

        static SST()
        {
            RefreshSceneList();
            SelectCurrentScene();
            EditorSceneManager.activeSceneChangedInEditMode += (prev, current) => UpdateSceneSelection();
            EditorApplication.playModeStateChanged += OnPlayModeChanged;
            EditorApplication.delayCall += AddToolbarUI;
        }

        private static void AddToolbarUI()
        {
            var toolbarType = typeof(UnityEditor.Editor).Assembly.GetType("UnityEditor.Toolbar");
            if (toolbarType == null) return;

            var toolbars = Resources.FindObjectsOfTypeAll(toolbarType);
            if (toolbars.Length == 0) return;

            var toolbar = toolbars[0];
            var rootField = toolbarType.GetField("m_Root", BindingFlags.NonPublic | BindingFlags.Instance);
            if (rootField == null) return;

            var root = rootField.GetValue(toolbar) as VisualElement;
            if (root == null) return;

            var leftContainer = root.Q("ToolbarZoneLeftAlign");
            if (leftContainer == null) return;

            if (toolbarUI != null)
                leftContainer.Remove(toolbarUI);

            toolbarUI = new IMGUIContainer(OnGUI);
            toolbarUI.style.flexShrink = 0;
            leftContainer.Add(toolbarUI);
        }

        private static void OnGUI()
        {
            CheckAndRefreshScenes();

            if (selectedIndex >= sceneNames.Length)
                selectedIndex = 0;

            var isPlaying = EditorApplication.isPlaying;

            GUILayout.BeginHorizontal();

            EditorGUI.BeginDisabledGroup(isPlaying);
            var newFetchAllScenes =
                GUILayout.Toggle(FetchAllScenes, "All Scenes", "Button", GUILayout.Height(dropdownBoxHeight));

            if (newFetchAllScenes != FetchAllScenes)
            {
                FetchAllScenes = newFetchAllScenes;
                RefreshSceneList();
                SelectCurrentScene();
            }

            EditorGUI.EndDisabledGroup();

            EditorGUI.BeginDisabledGroup(isPlaying);
            var popupStyle = new GUIStyle(EditorStyles.popup) { fixedHeight = dropdownBoxHeight };
            var newIndex = EditorGUILayout.Popup(selectedIndex, sceneNames, popupStyle, GUILayout.Width(150),
                GUILayout.Height(dropdownBoxHeight));
            if (newIndex != selectedIndex)
            {
                selectedIndex = newIndex;
                LoadScene(sceneNames[selectedIndex]);
            }

            EditorGUI.EndDisabledGroup();

            if (GUILayout.Button("Boot ▶", GUILayout.Height(dropdownBoxHeight), GUILayout.Width(60)))
                RunBoot();

            GUILayout.EndHorizontal();
        }

        private static void RunBoot()
        {
            if (!File.Exists(BootScenePath))
            {
                Debug.LogError("Boot scene not found at path: " + BootScenePath);
                return;
            }

            if (EditorApplication.isPlaying)
            {
                pendingBootPlay = true;
                EditorApplication.isPlaying = false;
                return;
            }

            if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
                return;

            EditorSceneManager.OpenScene(BootScenePath);
            EditorApplication.isPlaying = true;
        }

        private static void RefreshSceneList()
        {
            if (FetchAllScenes)
            {
                sceneNames = Directory.GetFiles("Assets", "*.unity", SearchOption.AllDirectories)
                    .Select(Path.GetFileNameWithoutExtension)
                    .Distinct()
                    .ToArray();
            }
            else
            {
                sceneNames = EditorBuildSettings.scenes
                    .Where(s => s.enabled)
                    .Select(s => Path.GetFileNameWithoutExtension(s.path))
                    .Distinct()
                    .ToArray();
            }
        }

        private static void CheckAndRefreshScenes()
        {
            string[] currentScenes;
            if (FetchAllScenes)
            {
                currentScenes = Directory.GetFiles("Assets", "*.unity", SearchOption.AllDirectories)
                    .Select(Path.GetFileNameWithoutExtension)
                    .Distinct()
                    .ToArray();
            }
            else
            {
                currentScenes = EditorBuildSettings.scenes
                    .Where(s => s.enabled)
                    .Select(s => Path.GetFileNameWithoutExtension(s.path))
                    .Distinct()
                    .ToArray();
            }

            if (!currentScenes.SequenceEqual(sceneNames))
            {
                sceneNames = currentScenes;
                SelectCurrentScene();
            }
        }

        private static void SelectCurrentScene()
        {
            var currentScene = Path.GetFileNameWithoutExtension(EditorSceneManager.GetActiveScene().path);
            var index = System.Array.IndexOf(sceneNames, currentScene);

            if (index != -1)
            {
                selectedIndex = index;
                lastActiveScene = currentScene;
            }
            else
            {
                var notInBuildName = currentScene + " (not in build index)";
                sceneNames = new[] { notInBuildName }.Concat(sceneNames).ToArray();
                selectedIndex = 0;
                lastActiveScene = currentScene;
            }
        }

        private static void UpdateSceneSelection()
        {
            var currentScene = Path.GetFileNameWithoutExtension(SceneManager.GetActiveScene().path);
            if (currentScene != lastActiveScene)
            {
                lastActiveScene = currentScene;
                sceneNames = sceneNames.Where(name => !name.EndsWith(" (not in build index)")).ToArray();
                SelectCurrentScene();
            }
        }

        private static void LoadScene(string sceneName)
        {
            string scenePath;

            if (FetchAllScenes)
                scenePath = Directory.GetFiles("Assets", "*.unity", SearchOption.AllDirectories)
                    .FirstOrDefault(path => Path.GetFileNameWithoutExtension(path) == sceneName);
            else
                scenePath = EditorBuildSettings.scenes
                    .FirstOrDefault(s => s.enabled && Path.GetFileNameWithoutExtension(s.path) == sceneName)?.path;

            if (!string.IsNullOrEmpty(scenePath))
            {
                if (EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
                    EditorSceneManager.OpenScene(scenePath);
            }
            else
                Debug.LogError("Scene not found: " + sceneName);
        }

        private static void OnPlayModeChanged(PlayModeStateChange state)
        {
            if (state is PlayModeStateChange.EnteredPlayMode or PlayModeStateChange.ExitingPlayMode)
                EditorApplication.delayCall += AddToolbarUI;

            if (state != PlayModeStateChange.EnteredEditMode || !pendingBootPlay)
                return;

            pendingBootPlay = false;

            if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
                return;

            EditorSceneManager.OpenScene(BootScenePath);
            EditorApplication.isPlaying = true;
        }
    }
}
#endif