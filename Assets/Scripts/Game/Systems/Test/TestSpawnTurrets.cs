using Unity.Entities;
using Unity.Mathematics;
using Unity.Collections;
using Unity.Transforms;

namespace Game.ECS
{
    public partial struct TestSpawnTurrets : ISystem
    {
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<GameManager>();
        }

        public void OnUpdate(ref SystemState state)
        {
            state.Enabled = false;

            var gameManager = SystemAPI.GetSingleton<GameManager>();

            //state.EntityManager.Instantiate(gameManager.turret);

            var instances = state.EntityManager.Instantiate(gameManager.turret, gameManager.countSpawn, Allocator.Temp);

            var random = Random.CreateFromIndex(123);

            foreach (var entity in instances)
            {
                var transform = SystemAPI.GetComponentRW<LocalTransform>(entity);
                transform.ValueRW.Position = random.NextFloat3Direction() * random.NextFloat(5, 10);
                //transform.ValueRW.Rotation = random.NextQuaternionRotation();
            }
        }
    }
}