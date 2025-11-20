#if UNITY_EDITOR
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using NaughtyAttributes;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace AbyssMoth
{
    [Flags]
    public enum ValidationScope
    {
        None = 0,
        OpenScenes = 1 << 0,
        Prefabs = 1 << 1,
        ScriptableObjects = 1 << 2
    }

    public sealed class ValidationIssue
    {
        public UnityEngine.Object Target;
        public string Path;
        public string ComponentType;
        public string FieldName;
        public string Message;
    }

    public static class RequiredReferenceValidator
    {
        public static List<ValidationIssue> Validate(ValidationScope scope, RequiredValidationSettings settings)
        {
            var issues = new List<ValidationIssue>();
            var keys = new HashSet<string>();
            if ((scope & ValidationScope.OpenScenes) != 0) ValidateOpenScenes(settings, issues, keys);
            if ((scope & ValidationScope.Prefabs) != 0) ValidatePrefabs(settings, issues, keys);
            if ((scope & ValidationScope.ScriptableObjects) != 0) ValidateScriptableObjects(settings, issues, keys);
            LogSummary(issues, settings);
            return issues;
        }

        static void ValidateOpenScenes(RequiredValidationSettings settings, List<ValidationIssue> issues, HashSet<string> keys)
        {
            var comps = UnityEngine.Object.FindObjectsByType<MonoBehaviour>(FindObjectsInactive.Include, FindObjectsSortMode.None);
            for (var i = 0; i < comps.Length; i++)
            {
                var c = comps[i];
                if (!c) continue;
                if (!c.gameObject.scene.IsValid()) continue;
                ValidateObject(c, GetTransformPath(c.transform), settings, issues, keys);
            }
        }

        static void ValidatePrefabs(RequiredValidationSettings settings, List<ValidationIssue> issues, HashSet<string> keys)
        {
            var guids = AssetDatabase.FindAssets("t:Prefab");
            for (var i = 0; i < guids.Length; i++)
            {
                var path = AssetDatabase.GUIDToAssetPath(guids[i]);
                var go = AssetDatabase.LoadAssetAtPath<GameObject>(path);
                if (!go) continue;
                var comps = go.GetComponentsInChildren<MonoBehaviour>(true);
                for (var j = 0; j < comps.Length; j++)
                {
                    var c = comps[j];
                    if (!c) continue;
                    ValidateObject(c, path, settings, issues, keys);
                }
            }
        }

        static void ValidateScriptableObjects(RequiredValidationSettings settings, List<ValidationIssue> issues, HashSet<string> keys)
        {
            var guids = AssetDatabase.FindAssets("t:ScriptableObject");
            for (var i = 0; i < guids.Length; i++)
            {
                var path = AssetDatabase.GUIDToAssetPath(guids[i]);
                var so = AssetDatabase.LoadAssetAtPath<ScriptableObject>(path);
                if (!so) continue;
                ValidateObject(so, path, settings, issues, keys);
            }
        }

        static void ValidateObject(UnityEngine.Object obj, string path, RequiredValidationSettings settings, List<ValidationIssue> issues, HashSet<string> keys)
        {
            var flags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;
            var fields = obj.GetType().GetFields(flags);
            for (var i = 0; i < fields.Length; i++)
            {
                var f = fields[i];
                var hasRequired = f.GetCustomAttribute<RequiredAttribute>(true) != null || f.GetCustomAttribute<VAttribute>(true) != null;
                if (!hasRequired) continue;
                var ft = f.FieldType;
                if (typeof(UnityEngine.Object).IsAssignableFrom(ft))
                {
                    var val = (UnityEngine.Object)f.GetValue(obj);
                    if (!val) AddIssue(obj, path, f, "is not assigned", settings, issues, keys);
                    continue;
                }
                if (ft.IsArray)
                {
                    var et = ft.GetElementType();
                    if (typeof(UnityEngine.Object).IsAssignableFrom(et))
                    {
                        var arr = (Array)f.GetValue(obj);
                        if (arr == null || arr.Length == 0) { AddIssue(obj, path, f, "array is empty", settings, issues, keys); continue; }
                        for (var k = 0; k < arr.Length; k++)
                        {
                            var e = (UnityEngine.Object)arr.GetValue(k);
                            if (!e) { AddIssue(obj, path, f, "array contains null", settings, issues, keys); break; }
                        }
                    }
                    continue;
                }
                if (typeof(IList).IsAssignableFrom(ft) && ft.IsGenericType)
                {
                    var et = ft.GetGenericArguments()[0];
                    if (typeof(UnityEngine.Object).IsAssignableFrom(et))
                    {
                        var list = (IList)f.GetValue(obj);
                        if (list == null || list.Count == 0) { AddIssue(obj, path, f, "list is empty", settings, issues, keys); continue; }
                        for (var k = 0; k < list.Count; k++)
                        {
                            var e = (UnityEngine.Object)list[k];
                            if (!e) { AddIssue(obj, path, f, "list contains null", settings, issues, keys); break; }
                        }
                    }
                    continue;
                }
            }
        }

        static void AddIssue(UnityEngine.Object obj, string path, FieldInfo field, string reason, RequiredValidationSettings settings, List<ValidationIssue> issues, HashSet<string> keys)
        {
            var key = obj.GetInstanceID() + "|" + field.Name + "|" + path;
            if (!keys.Add(key)) return;
            var nice = ObjectNames.NicifyVariableName(field.Name);
            var msg = "[Required] «" + nice + "» " + reason + " on " + obj.GetType().Name + " (" + GetObjectLabel(obj, path) + ")";
            switch (settings.LogLevel)
            {
                case RequiredLogLevel.Error: Debug.LogError(msg, obj); break;
                case RequiredLogLevel.Warning: Debug.LogWarning(msg, obj); break;
            }
            var issue = new ValidationIssue
            {
                Target = obj,
                Path = path,
                ComponentType = obj.GetType().Name,
                FieldName = nice,
                Message = msg
            };
            issues.Add(issue);
        }

        static string GetTransformPath(Transform t)
        {
            var p = t.name;
            while (t && t.parent) { t = t.parent; p = t.name + "/" + p; }
            return p;
        }

        static string GetObjectLabel(UnityEngine.Object obj, string path)
        {
            if (obj is Component c) return "GameObject: " + GetTransformPath(c.transform);
            if (!string.IsNullOrEmpty(path)) return "Asset: " + Path.GetFileName(path);
            return obj.name;
        }

        static void LogSummary(List<ValidationIssue> issues, RequiredValidationSettings settings)
        {
            var level = settings.LogLevel;
            var text = "Validation: " + issues.Count + " issue(s)";
            if (issues.Count == 0) Debug.Log(text);
            else
            {
                if (level == RequiredLogLevel.Warning) Debug.LogWarning(text);
                else Debug.LogError(text);
            }
        }
    }

    public sealed class VAttribute : RequiredAttribute
    {
        public VAttribute(string message = null, RequiredLogLevel logLevel = RequiredLogLevel.Error) : base(message, logLevel) { }
    }
}
#endif
