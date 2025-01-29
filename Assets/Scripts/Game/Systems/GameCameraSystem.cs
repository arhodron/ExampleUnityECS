using UnityEngine;
using Unity.Entities;
using Unity.Mathematics;
using Game.Mono;

namespace Game.ECS
{
    /// <summary>
    /// TODO: 
    /// -rework/upgrade camera
    /// -include confg scriptable
    /// </summary>
    [System.Obsolete("rework")]
    public partial struct GameCameraSystem : ISystem
    {
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<GameManager>();

            state.EntityManager.AddComponent<GameCamera>(state.SystemHandle);
        }

        public void OnUpdate(ref SystemState state)
        {
            if (GameCameraMono.Instance == null)
                return;

            const float speedRotation = 10000;///to config
            const float distance = 5;///to config

            var camera = SystemAPI.GetSingleton<GameCamera>();

            var transform = GameCameraMono.Instance.transform;

            if (Input.GetMouseButton(1))
            {
                if (Input.GetAxis("Mouse X") != 0 || Input.GetAxis("Mouse Y") != 0)
                {
                    camera.angles.x += Input.GetAxis("Mouse X") * speedRotation * Time.deltaTime;
                    camera.angles.y -= Input.GetAxis("Mouse Y") * speedRotation * Time.deltaTime;
                }
            }

            camera.rotation = Quaternion.Euler(math.radians(camera.angles.y), math.radians(camera.angles.x), 0);
            camera.position = math.mul(camera.rotation, new float3(0, 0, -distance));

            transform.position = camera.position;
            transform.rotation = camera.rotation;

            var ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            camera.origin = ray.origin;
            camera.direction = ray.direction;

            SystemAPI.SetSingleton(camera);
        }
    }
}