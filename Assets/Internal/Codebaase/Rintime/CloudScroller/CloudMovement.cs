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

using System.Linq;
using UnityEngine;

namespace RimuruDev.Internal.Codebaase.Rintime.CloudScroller
{
    [SelectionBase]
    [DisallowMultipleComponent]
    public sealed class CloudMovement : MonoBehaviour
    {
        public float speed = 5f;
        public float teleportDistance = 10f;

        private void Update() =>
            ApplyMovementLeft();

        private void ApplyMovementLeft()
        {
            for (var i = 0; i < transform.childCount; i++)
            {
                var cloud = transform.GetChild(i);
                cloud.Translate(Vector3.left * (speed * Time.deltaTime));

                if (IsTeleportationReady(cloud))
                    ApplyTeleportation(cloud);
            }
        }

        private bool IsTeleportationReady(Transform cloud) =>
            cloud.position.x < -teleportDistance;

        private void ApplyTeleportation(Transform cloud)
        {
            var newPosition = cloud.position;

            newPosition.x = GetRightmostPositionX() + teleportDistance;
            cloud.position = newPosition;
        }

        /// <summary>
        /// If you're not used to LINQ. Example below.
        /// </summary>
        /// <code>
        /// foreach (var currentCamera in Camera.allCameras)
        /// {
        ///     if (currentCamera.enabled)
        ///     {
        ///         var cameraRightX = currentCamera.ScreenToWorldPoint(new Vector3(currentCamera.pixelWidth, 0, 0)).x;
        ///         
        ///         if (cameraRightX > rightmostX)
        ///             rightmostX = cameraRightX;
        ///     }
        /// }
        /// </code>
        /// <returns></returns>
        private static float GetRightmostPositionX()
        {
            return (from currentCamera in Camera.allCameras
                    where currentCamera.enabled
                    select currentCamera.ScreenToWorldPoint(new Vector3(currentCamera.pixelWidth, 0, 0)).x)
                .Prepend(float.MinValue)
                .Max();
        }
    }
}