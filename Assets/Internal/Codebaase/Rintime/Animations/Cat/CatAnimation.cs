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
using DG.Tweening;
using UnityEngine.UI;
using System.Collections;

namespace RimuruDev.Internal.Codebaase.Rintime.Animations.Cat
{
    [SelectionBase]
    [DisallowMultipleComponent]
    public sealed class CatAnimation : MonoBehaviour
    {
        public Image catImage;
        public float vibrateDuration = 0.2f;
        public float vibrateStrength = 20f;
        public Ease vibrateEase = Ease.OutBounce;
        private Vector3 originalPosition;
        private bool isVibrating;

        private void Start()
        {
            originalPosition = catImage.rectTransform.localPosition;
            isVibrating = false;
        }

        public void StartVibrateAnimation()
        {
            if (isVibrating)
                return;

            isVibrating = true;
            StartCoroutine(VibrateAnimation());
        }

        public void StopVibrateAnimation()
        {
            isVibrating = false;
            StopAllCoroutines();
            catImage.rectTransform.localPosition = originalPosition;
        }

        private IEnumerator VibrateAnimation()
        {
            while (isVibrating)
            {
                var randomSign = (Random.value > 0.5f) ? 1f : -1f;
                var targetPosition = originalPosition + new Vector3(randomSign * vibrateStrength, 0f, 0f);

                catImage.rectTransform.DOAnchorPos(targetPosition, vibrateDuration)
                    .SetEase(vibrateEase);

                yield return new WaitForSeconds(vibrateDuration);
            }
        }
    }
}