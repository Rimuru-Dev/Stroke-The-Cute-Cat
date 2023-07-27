namespace Internal.Codebaase
{
    using UnityEngine;
    using DG.Tweening;

    public class SunAnimation : MonoBehaviour
    {
        public RectTransform sunRectTransform;
        public RectTransform centerRectTransform;
        public Vector2 initialPosition;
        public Vector2 centerPosition;
        public float scaleMultiplier = 1.5f;
        public float animationDuration = 1f;

        public SpecialProgressBar specialProgressBar;

        private void Start()
        {
            initialPosition = sunRectTransform.anchoredPosition;
            centerPosition = centerRectTransform.anchoredPosition;

            specialProgressBar.OnResetProgressBarAndChangeHeroImage += PlayAnimation;
        }

        private void OnDestroy()
        {
            specialProgressBar.OnResetProgressBarAndChangeHeroImage -= PlayAnimation;
        }

        private void PlayAnimation()
        {
            // Плавное перемещение солнца в центр экрана
            sunRectTransform.DOAnchorPos(centerPosition, animationDuration)
                .SetEase(Ease.OutQuad);

            // Плавное увеличение размера солнца
            sunRectTransform.DOScale(Vector3.one * scaleMultiplier, animationDuration / 2f)
                .SetEase(Ease.OutQuad)
                .SetDelay(animationDuration / 2f)
                .OnComplete(() =>
                {
                    // Плавное возвращение размера солнца к изначальному
                    sunRectTransform.DOScale(Vector3.one, animationDuration / 2f)
                        .SetEase(Ease.OutQuad)
                        .OnComplete(() =>
                        {
                            // Плавное перемещение солнца в изначальную позицию
                            sunRectTransform.DOAnchorPos(initialPosition, animationDuration)
                                .SetEase(Ease.OutQuad);
                        });
                });
        }
    }
}