// #if UNITY_EDITOR
// using System;
// using System.Collections.Generic;
// using System.Reflection;
// using UnityEditor;
// using UnityEditor.SceneManagement;
// using UnityEngine;
// using Object = UnityEngine.Object;
//
// namespace NaughtyAttributes.Editor
// {
//     [InitializeOnLoad]
//     public static class RequiredSceneValidator
//     {
//         // чтобы не спамить, логируем по ключу один раз
//         private static readonly HashSet<string> Logged = new();
//
//         static RequiredSceneValidator()
//         {
//             // Триггеры проверки
//             EditorApplication.playModeStateChanged += _ => DebouncedValidate();
//             EditorApplication.hierarchyChanged     += DebouncedValidate;
//             EditorSceneManager.sceneOpened         += (_, __) => DebouncedValidate();
//             // чуть подождать, чтобы не дергать слишком часто
//             DebouncedValidate();
//         }
//
//         private static void DebouncedValidate()
//         {
//             EditorApplication.delayCall -= ValidateOpenScenes;
//             EditorApplication.delayCall += ValidateOpenScenes;
//         }
//
//         public static void ValidateOpenScenes()
//         {
//             // Можно чистить, чтобы заново логировало при новой загрузке
//             Logged.Clear();
//
//             var comps = Object.FindObjectsByType<MonoBehaviour>(
//                 FindObjectsInactive.Include, FindObjectsSortMode.None);
//
//             foreach (var mb in comps)
//             {
//                 if (!mb || mb.gameObject.scene.IsValid() == false) continue;
//
//                 ValidateComponent(mb);
//             }
//         }
//
//         private static void ValidateComponent(MonoBehaviour mb)
//         {
//             var flags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;
//             var fields = mb.GetType().GetFields(flags);
//
//             foreach (var f in fields)
//             {
//                 var req = f.GetCustomAttribute<RequiredAttribute>(true);
//                 if (req == null) continue;
//
//                 // интересуют только Object-поля
//                 if (!typeof(Object).IsAssignableFrom(f.FieldType)) continue;
//
//                 var val = (Object) f.GetValue(mb);
//                 if (val) continue;
//
//                 // ключ "компонент+поле"
//                 var key = $"{mb.GetInstanceID()}|{f.Name}";
//                 if (!Logged.Add(key)) continue;
//
//                 var nice = ObjectNames.NicifyVariableName(f.Name);
//                 var msg  = $"[Required] «{nice}» is not assigned on {mb.GetType().Name} " +
//                            $"(GameObject: {GetPath(mb.transform)})";
//
//                 // можно расширить твоим enum RequiredLogLevel
//                 Debug.LogError(msg, mb);
//             }
//         }
//
//         private static string GetPath(Transform t)
//         {
//             var path = t.name;
//             while (t && t.parent) { t = t.parent; path = t.parent.name + "/" + path; }
//             return path;
//         }
//     }
// }
// #endif
