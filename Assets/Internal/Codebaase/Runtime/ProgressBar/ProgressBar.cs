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

using System;
using Plugins.Audio.Core;
using RimuruDev.Internal.Codebaase.Runtime.Animations.Cat;
using UnityEngine;
using UnityEngine.UI;

namespace RimuruDev.Internal.Codebaase.Runtime.ProgressBar
{
    [SelectionBase]
    [DisallowMultipleComponent]
    public sealed class ProgressBar : MonoBehaviour
    {
        private const int IncreaseCatIndex = 1;
        private const string Cataudio = "CatAudio";

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
        private Vector3 previousCursorPosition;

        private void Start()
        {
            Screen.orientation = ScreenOrientation.Portrait;
            {
                leftFillImage.fillAmount = 0f;
                rightFillImage.fillAmount = 0f;

                catIndex += IncreaseCatIndex;

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
            if (Input.GetMouseButton(0))
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

        private void ResetProgressBarAndChangeHeroImage()
        {
            OnResetProgressBarAndChangeHeroImage?.Invoke();

            catIndex += IncreaseCatIndex;

            if (catIndex < maxCatLength)
                cat.sprite = cats[catIndex];
            else
            {
                catIndex = 0;
                cat.sprite = cats[catIndex];
            }
        }

        // TODO: Move to Audio Handler

        private void PlayCatSound()
        {
            if (!catSoundPlaying)
            {
                sourceAudio.Play(Cataudio);
                catSoundPlaying = true;
            }
        }

        private void StopCatSound()
        {
            if (catSoundPlaying)
            {
                sourceAudio.Stop();
                catSoundPlaying = false;
            }
        }
    }
}