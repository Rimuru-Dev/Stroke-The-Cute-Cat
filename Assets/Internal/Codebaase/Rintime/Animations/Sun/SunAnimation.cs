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

using DG.Tweening;
using UnityEngine;
using UnityEngine.Serialization;

namespace RimuruDev.Internal.Codebaase.Rintime.Animations.Sun
{
    [SelectionBase]
    [DisallowMultipleComponent]
    public class SunAnimation : MonoBehaviour
    {
        public RectTransform sunRectTransform;
        public RectTransform centerRectTransform;
        public Vector2 initialPosition;
        public Vector2 centerPosition;
        public float scaleMultiplier = 1.5f;
        public float animationDuration = 1f;

        [FormerlySerializedAs("specialProgressBar")]
        public ProgressBar.ProgressBar progressBar;

        private void Start()
        {
            initialPosition = sunRectTransform.anchoredPosition;
            centerPosition = centerRectTransform.anchoredPosition;

            progressBar.OnResetProgressBarAndChangeHeroImage += PlayAnimation;
        }

        private void OnDestroy() =>
            progressBar.OnResetProgressBarAndChangeHeroImage -= PlayAnimation;

        private void PlayAnimation()
        {
            sunRectTransform.DOAnchorPos(centerPosition, animationDuration)
                .SetEase(Ease.OutQuad);

            sunRectTransform.DOScale(Vector3.one * scaleMultiplier, animationDuration / 2f)
                .SetEase(Ease.OutQuad)
                .SetDelay(animationDuration / 2f)
                .OnComplete(() =>
                {
                    sunRectTransform.DOScale(Vector3.one, animationDuration / 2f)
                        .SetEase(Ease.OutQuad)
                        .OnComplete(() =>
                        {
                            sunRectTransform.DOAnchorPos(initialPosition, animationDuration)
                                .SetEase(Ease.OutQuad);
                        });
                });
        }
    }
}