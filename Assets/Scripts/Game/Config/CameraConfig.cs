using UnityEngine;

namespace Game
{
    [CreateAssetMenu(fileName = nameof(CameraConfig), menuName = "Configs/" + nameof(CameraConfig))]
    public class CameraConfig : ScriptableObject
    {
        [SerializeField]
        private float speedRotation = 10;
        public float SpeedRotation => speedRotation;
        [SerializeField]
        private float distance = 10;
        public float Distance => distance;
    }
}
