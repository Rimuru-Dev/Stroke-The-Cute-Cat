// ReSharper disable CommentTypo
// **************************************************************** //
//
//   Copyright (c) RimuruDev. All rights reserved.
//   Contact me: 
//          - Gmail:    rimuru.dev@gmail.com
//          - GitHub:   https://github.com/RimuruDev
//          - LinkedIn: https://www.linkedin.com/in/rimuru/
//
// **************************************************************** //

using UnityEngine;

namespace RimuruDev.Internal.Codebaase.Runtime.Animations.Sun
{
    [DisallowMultipleComponent]
    public sealed class RotateSun : MonoBehaviour
    {
        public float rotationSpeed = 10f;

        private void Update() =>
            transform.Rotate(Vector3.forward, rotationSpeed * Time.deltaTime);
    }
}