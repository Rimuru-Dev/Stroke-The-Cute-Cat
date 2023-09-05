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
using UnityEngine.UI;

namespace RimuruDev.Internal.Codebaase.Runtime.Background
{
    [SelectionBase]
    [DisallowMultipleComponent]
    [RequireComponent(typeof(RawImage))]
    public sealed class BackgroundScrolling : MonoBehaviour
    {
        [SerializeField] private LoopMode loopMode = LoopMode.Update;
        [SerializeField] private Vector2 scrollPositionXY;
        [SerializeField] private RawImage rawImage;

        private void Start() =>
            rawImage ??= GetComponent<RawImage>();

        private void FixedUpdate()
        {
            if (loopMode != LoopMode.FixedUpdate)
                return;

            SetNewRawImageUV(Time.fixedTime);
        }

        private void Update()
        {
            if (loopMode != LoopMode.Update)
                return;

            SetNewRawImageUV(Time.deltaTime);
        }

        private void LateUpdate()
        {
            if (loopMode != LoopMode.LateUpdate)
                return;

            SetNewRawImageUV(Time.deltaTime);
        }

        private void SetNewRawImageUV(float delta) =>
            rawImage.uvRect = CalculateRectPosition(delta);

        private Rect CalculateRectPosition(float delta)
        {
            var offset = new Vector2(scrollPositionXY.x, scrollPositionXY.y);
            var position = rawImage.uvRect.position + offset * delta;
            var size = rawImage.uvRect.size;

            return new Rect(position, size);
        }
    }
}