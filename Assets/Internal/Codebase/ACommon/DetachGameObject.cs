using UnityEngine;

namespace AbyssMoth.Internal.Codebase.ACommon
{
    [DisallowMultipleComponent]
    public sealed class DetachGameObject : MonoBehaviour
    {
        private void Awake() => 
            transform.SetParent(null);
    }
}