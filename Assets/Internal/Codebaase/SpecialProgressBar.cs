using System;
using Plugins.Audio.Core;
using UnityEngine;
using UnityEngine.UI;

namespace Internal.Codebaase
{
    public sealed class SpecialProgressBar : MonoBehaviour
    {
        public Action OnResetProgressBarAndChangeHeroImage;

        public Image leftFillImage;
        public Image rightFillImage;
        public float fillSpeed = 0.001f;
        public float resetThreshold = 0.99f;
        public AudioSource catSound;
        public SourceAudio sourceAudio;

        public Image emojy;
        public Sprite eyeEmojy;
        public Sprite cawayEmojy;

        public Image cat;

        public Sprite[] cats;
        public int catIndex = 0;
        public int maxCatLength;

        public CatAnimation catAnimation;

        private bool isTouching;
        private bool catSoundPlaying;

        private float speedThreshold = 0f;
        private Vector3 previousCursorPosition;

        private void Start()
        {
            Screen.orientation = ScreenOrientation.Portrait;
            //ResetProgressBar();
            {
                leftFillImage.fillAmount = 0f;
                rightFillImage.fillAmount = 0f;
                catIndex += 1;

                if (catIndex < maxCatLength)
                    cat.sprite = cats[catIndex];
                else
                {
                    catIndex = 0;
                    cat.sprite = cats[catIndex];
                }
            }

            maxCatLength = cats.Length;
            cat.sprite = cats[catIndex];
        }

        private void Update()
        {
            if (Input.GetMouseButton(0) && IsCursorInsideScreen())
            {
                if (rightFillImage.fillAmount >= resetThreshold)
                    ResetProgressBar();

                if (Input.mousePosition != previousCursorPosition)
                {
                    isTouching = true;

                    PlayCatSound();

                    emojy.sprite = cawayEmojy;

                    catAnimation.StartVibrateAnimation();
                }
                else
                {
                    isTouching = false;
                }

                previousCursorPosition = Input.mousePosition;

                if (isTouching)
                    UpdateProgressBar();
            }
            else
            {
                isTouching = false;

                StopCatSound();

                emojy.sprite = eyeEmojy;

                catAnimation.StopVibrateAnimation();

                if (rightFillImage.fillAmount >= resetThreshold)
                    ResetProgressBar();
            }
        }

        private bool IsCursorInsideScreen()
        {
            // Vector3 cursorPosition = Input.mousePosition;

            // if (cursorPosition.x < 0 || cursorPosition.x > Screen.width ||
            //     cursorPosition.y < 0 || cursorPosition.y > Screen.height)
            // {
            //     return false;
            // }

            return true;
        }

        private void ResetProgressBar()
        {
            leftFillImage.fillAmount = 0f;
            rightFillImage.fillAmount = 0f;

            ResetProgressBarAndChangeHeroImage();
        }

        private void UpdateProgressBar()
        {
            var moveAmount = GetMoveAmount();

            leftFillImage.fillAmount = Mathf.Clamp01(leftFillImage.fillAmount + moveAmount * fillSpeed);
            rightFillImage.fillAmount = Mathf.Clamp01(rightFillImage.fillAmount + moveAmount * fillSpeed);
        }

        private float GetMoveAmount()
        {
            var moveDelta = Vector2.zero;

            if (isTouching)
                moveDelta = Input.mousePosition - new Vector3(Screen.width * 0.5f, Screen.height * 0.5f);

            var moveAmount = moveDelta.magnitude / Screen.width;

            return moveAmount;
        }

        private void PlayCatSound()
        {
            if (!catSoundPlaying)
            {
                //  catSound.Play();
                sourceAudio.Play("CatAudio");
                catSoundPlaying = true;
            }
        }

        private void StopCatSound()
        {
            if (catSoundPlaying)
            {
                // catSound.Stop();
                sourceAudio.Stop();
                catSoundPlaying = false;
            }
        }

        private void ResetProgressBarAndChangeHeroImage()
        {
            OnResetProgressBarAndChangeHeroImage?.Invoke();

            catIndex += 1;

            if (catIndex < maxCatLength)
                cat.sprite = cats[catIndex];
            else
            {
                catIndex = 0;
                cat.sprite = cats[catIndex];
            }
        }
    }
}