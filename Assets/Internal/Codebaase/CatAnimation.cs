using System.Collections;

namespace Internal.Codebaase
{
    using UnityEngine;
    using UnityEngine.UI;
    using DG.Tweening;

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
            if (!isVibrating)
            {
                isVibrating = true;
                StartCoroutine(VibrateAnimation());
            }
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
                float randomSign = (Random.value > 0.5f) ? 1f : -1f;
                Vector3 targetPosition = originalPosition + new Vector3(randomSign * vibrateStrength, 0f, 0f);

                catImage.rectTransform.DOAnchorPos(targetPosition, vibrateDuration)
                    .SetEase(vibrateEase);

                yield return new WaitForSeconds(vibrateDuration);
            }
        }
    }
}