using UnityEngine;

namespace Internal.Codebaase
{
    public class CloudMovement : MonoBehaviour
    {
        public float speed = 5f; // Скорость движения облаков
        public float teleportDistance = 10f; // Расстояние, при котором облака телепортируются влево

        private void Update()
        {
            // Перемещаем все дочерние объекты облака влево с заданной скоростью
            for (int i = 0; i < transform.childCount; i++)
            {
                Transform cloud = transform.GetChild(i);
                cloud.Translate(Vector3.left * (speed * Time.deltaTime));

                // Проверяем, вышло ли облако за экран на заданное растояние, и телепортируем его обратно влево
                if (cloud.position.x < -teleportDistance)
                {
                    Vector3 newPosition = cloud.position;
                    newPosition.x = GetRightmostPositionX() + teleportDistance;
                    cloud.position = newPosition;
                }
            }
        }

        private float GetRightmostPositionX()
        {
            float rightmostX = float.MinValue;

            // Находим координату X самого правого пикселя на экране
            foreach (Camera camera in Camera.allCameras)
            {
                if (camera.enabled)
                {
                    float cameraRightX = camera.ScreenToWorldPoint(new Vector3(camera.pixelWidth, 0, 0)).x;
                    if (cameraRightX > rightmostX)
                        rightmostX = cameraRightX;
                }
            }

            return rightmostX;
        }
    }
}