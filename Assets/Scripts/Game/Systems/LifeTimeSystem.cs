using Unity.Entities;
using Unity.Burst;

namespace Game.ECS
{
    [BurstCompile]
    [RequireMatchingQueriesForUpdate]
    [UpdateInGroup(typeof(SimulationSystemGroup))]
    public partial struct LifeTimeSystem : ISystem
    {
        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            float deltaTime = SystemAPI.Time.DeltaTime;

            var ecb = SystemAPI.GetSingleton<EndSimulationEntityCommandBufferSystem.Singleton>().CreateCommandBuffer(state.WorldUnmanaged);

            foreach ((var lifetime, var entity) in SystemAPI.Query<RefRW<LifeTime>>().WithEntityAccess())
            {
                ref var val = ref lifetime.ValueRW;
                val.value -= deltaTime;
                if (val.value < 0)
                    ecb.AddComponent<DestroyEvent>(entity);
            }
        }
    }
}