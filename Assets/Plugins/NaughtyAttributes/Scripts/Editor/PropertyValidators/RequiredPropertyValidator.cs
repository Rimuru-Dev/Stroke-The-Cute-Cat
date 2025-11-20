// RimuruDev modification 23.10.2025 21:12(+4->s)
#if UNITY_EDITOR
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace NaughtyAttributes.Editor
{
    public sealed class RequiredPropertyValidator : PropertyValidatorBase
    {
        private static readonly HashSet<string> logged = new();

        public override void ValidateProperty(SerializedProperty property)
        {
            if (IsPrefabContext(property.serializedObject.targetObject))
                return;

            if (property.propertyType != SerializedPropertyType.ObjectReference)
                return;

            if (property.objectReferenceValue != null)
                return;

            var target = property.serializedObject.targetObject;
            var attr = PropertyUtility.GetAttribute<RequiredAttribute>(property);
            var message = string.IsNullOrEmpty(attr?.Message) ? property.displayName + " is required" : attr.Message;

            NaughtyEditorGUI.HelpBox_Layout(message, MessageType.Error, context: target);

            if (attr is { LogLevel: not RequiredLogLevel.None } && Event.current != null && Event.current.type == EventType.Repaint)
            {
                var key = target.GetInstanceID() + "|" + property.propertyPath;
                if (logged.Add(key))
                {
                    var path = GetObjectPath(target as Component);
                    var msg = "[Required] «" + property.displayName + "» is not assigned on " +
                              target.GetType().Name + " (GameObject: " + path + ")";
                    if (attr.LogLevel == RequiredLogLevel.Error) Debug.LogError(msg, target);
                    else Debug.LogWarning(msg, target);
                }
            }
        }

        private static bool IsPrefabContext(Object target)
        {
            if (!target) return false;
            if (PrefabStageUtility.GetCurrentPrefabStage() != null) return true;
            if (EditorUtility.IsPersistent(target)) return true;

            if (target is Component c)
            {
                if (!c.gameObject.scene.IsValid()) return true;
                if (EditorUtility.IsPersistent(c.gameObject)) return true;
                if (PrefabUtility.IsPartOfPrefabAsset(c)) return true;
                return false;
            }

            if (target is GameObject go)
            {
                if (!go.scene.IsValid()) return true;
                if (EditorUtility.IsPersistent(go)) return true;
                if (PrefabUtility.IsPartOfPrefabAsset(go)) return true;
                return false;
            }

            return false;
        }

        private static string GetObjectPath(Component c)
        {
            if (!c) return "null";
            var t = c.transform;
            var path = t.name;
            while (t.parent) { t = t.parent; path = t.name + "/" + path; }
            return path;
        }
    }
}
#endif
