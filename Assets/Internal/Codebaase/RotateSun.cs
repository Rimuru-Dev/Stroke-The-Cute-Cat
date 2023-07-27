namespace Internal.Codebaase
{
    using UnityEngine;

    [SelectionBase]
    [DisallowMultipleComponent]
    public sealed class RotateSun : MonoBehaviour
    {
        public float rotationSpeed = 10f;

        private void Update() =>
            transform.Rotate(Vector3.forward, rotationSpeed * Time.deltaTime);
    }
}