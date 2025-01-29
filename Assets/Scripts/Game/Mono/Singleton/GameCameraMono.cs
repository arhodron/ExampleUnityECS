using UnityEngine;

namespace Game.Mono
{
    [RequireComponent(typeof(Camera))]
    public class GameCameraMono : MonoBehaviour
    {
        public static GameCameraMono Instance { private set; get; } = null;

        private void Awake()
        {
            Instance = this;
        }
    }
}
