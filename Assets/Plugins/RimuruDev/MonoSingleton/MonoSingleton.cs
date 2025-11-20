// **************************************************************** //
//
//   Copyright (c) RimuruDev. All rights reserved.
//   Contact me: 
//          - Gmail:    rimuru.dev@gmail.com
//          - LinkedIn: https://www.linkedin.com/in/rimuru/
//          - GitHub:   https://github.com/RimuruDev
//
// **************************************************************** //

using System.Diagnostics.CodeAnalysis;
using UnityEngine;
using UnityEngine.Scripting;

namespace AbyssMoth
{
    [Preserve]
    [HelpURL("https://github.com/RimuruDev/MonoSingleton")]
    [SuppressMessage("ReSharper", "UnusedMember.Global")]
    [SuppressMessage("ReSharper", "StaticMemberInGenericType")]
    [SuppressMessage("ReSharper", "MemberCanBePrivate.Global")]
    public abstract class MonoSingleton<TComponent> : MonoBehaviour where TComponent : Component
    {
        [Header("DontDestroyOnLoad")]
        [SerializeField] private bool EnableDontDestroyOnLoad = true;
        
        [Preserve] private static volatile TComponent instance;
        [Preserve] private static readonly object lockObject = new();

        [Preserve] public static bool HasInstance => instance != null;

        [Preserve]
        public static TComponent TryGetInstance() => HasInstance ? instance : null;

        [Preserve]
        public static TComponent Instance
        {
            get
            {
                if (instance != null)
                    return instance;

                lock (lockObject)
                {
                    if (instance == null)
                    {
                        instance = FindFirstObjectByType<TComponent>();

                        if (instance == null)
                        {
                            var obj = new GameObject
                            {
                                name = $"[ === {typeof(TComponent).Name} === ]"
                            };

                            instance = obj.AddComponent<TComponent>();
                        }
                    }

                    return instance;
                }
            }
        }

        [Preserve]
        protected virtual void Awake() =>
            InitializeSingleton();

        [Preserve]
        protected virtual void InitializeSingleton()
        {
            if (!Application.isPlaying)
                return;

            lock (lockObject)
            {
                if (instance == null)
                {
                    instance = this as TComponent;
                    transform.SetParent(null);

                    if (EnableDontDestroyOnLoad)
                        DontDestroyOnLoad(gameObject);
                }
                else if (instance != this)
                {
                    Debug.LogError(
                        $"Another instance of {typeof(TComponent).Name} already exists. Destroying the duplicate.");
                    Destroy(gameObject);
                }
            }
        }
    }
}