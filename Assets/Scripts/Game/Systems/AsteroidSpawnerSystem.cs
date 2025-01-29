using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using Unity.Burst;
using Random = Unity.Mathematics.Random;

namespace Game.ECS
{
    [BurstCompile]
    [RequireMatchingQueriesForUpdate]
    [UpdateInGroup(typeof(SimulationSystemGroup))]
    public partial struct AsteroidSpawnerSystem : ISystem
    {
        private Random random;

        public void OnCreate(ref SystemState state)
        {
            random = new Random(123);
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            double time = SystemAPI.Time.ElapsedTime;

            
            var ecb = SystemAPI.GetSingleton<BeginSimulationEntityCommandBufferSystem.Singleton>().CreateCommandBuffer(state.WorldUnmanaged);

            foreach ((var spawner, var transform) in SystemAPI.Query<RefRW<AsteroidSpawner>, RefRO<LocalTransform>>())
            {
                ref var spawn = ref spawner.ValueRW;
                if (spawn.spawnRate == 0)
                    continue;

                float spawnTime = 1 / spawn.spawnRate;
                if (spawn.timeRate + spawnTime < time)
                {
                    spawn.timeRate = time;

                    var prefab = spawner.ValueRW.prefab;
                    var entity = ecb.Instantiate(prefab);

                    float3 position = transform.ValueRO.Position;
                    float3 start = math.mul(random.NextQuaternionRotation(), new float3(spawner.ValueRO.rangeSpawn, 0, 0)) + position;
                    float3 end = math.mul(random.NextQuaternionRotation(), new float3(random.NextFloat(0, spawner.ValueRO.rangeTarget), 0, 0)) + position;

                    var tr = SystemAPI.GetComponent<LocalTransform>(prefab);
                    tr.Position = start;
                    ecb.SetComponent(entity, tr);

                    var move = SystemAPI.GetComponent<Move>(prefab);
                    var speed = SystemAPI.GetComponent<Speed>(prefab);
                    move.direction = math.normalize((end - start)) * speed.value;
                    ecb.SetComponent(entity, move);

                    ecb.SetComponent(entity, new LifeTime()
                    {
                        value = (spawner.ValueRO.rangeSpawn * 2) / speed.value,
                    });
                }
            }
        }
    }
}