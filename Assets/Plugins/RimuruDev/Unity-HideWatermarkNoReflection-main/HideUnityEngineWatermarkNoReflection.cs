#if !UNITY_EDITOR
#define ENABLE_DEBUG_LOG
#endif

using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Scripting;

namespace RimuruDev
{
    [Preserve]
    public sealed class HideUnityEngineWatermarkNoReflection
    {
        [Preserve]
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterAssembliesLoaded)]
        private static void HideWatermarkAssemblies()
        {
            Watermark.showDeveloperWatermark = false;
#if ENABLE_DEBUG_LOG
            Debug.Log("<color=yellow>[Watermark] OFF (AfterAssembliesLoaded)</color>");
#endif
        }

        [Preserve]
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void HideWatermarkBeforeScene()
        {
            Watermark.showDeveloperWatermark = false;
#if ENABLE_DEBUG_LOG
            Debug.Log("<color=yellow>[Watermark] OFF (BeforeSceneLoad)</color>");
#endif
        }

        [Preserve]
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void HideWatermarkAfterScene()
        {
            Watermark.showDeveloperWatermark = false;
#if ENABLE_DEBUG_LOG
            Debug.Log("<color=yellow>[Watermark] OFF (AfterSceneLoad)</color>");
#endif
        }
    }
}
