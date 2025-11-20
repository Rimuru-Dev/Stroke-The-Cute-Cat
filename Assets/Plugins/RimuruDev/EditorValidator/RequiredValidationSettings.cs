using NaughtyAttributes;
using UnityEngine;

namespace AbyssMoth
{
    //[CreateAssetMenu(menuName = "Create RequiredValidationSettings", fileName = "RequiredValidationSettings", order = 0)]
    public sealed class RequiredValidationSettings : ScriptableObject
    {
        public bool ValidateOnPlay = true;
        public bool CancelPlayOnError = true;
        public bool ValidateOnSceneOpen = false;
        public bool ValidateByTimer = false;
        public float TimerSeconds = 10f;
        public bool ScanOpenScenes = true;
        public bool ScanPrefabs = false;
        public bool ScanScriptableObjects = false;
        public RequiredLogLevel LogLevel = RequiredLogLevel.Error;

        public static RequiredValidationSettings Load()
        {
            var s = Resources.Load<RequiredValidationSettings>("RequiredValidationSettings");
            return s;
        }
    }
}