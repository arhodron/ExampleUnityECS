using Unity.Entities;
using Unity.Burst;

namespace Game.ECS
{
    /// <summary>
    /// TODO: ignore destroy event in other systems
    /// </summary>
    [BurstCompile]
    [RequireMatchingQueriesForUpdate]
    [UpdateInGroup(typeof(SimulationSystemGroup), OrderLast = true)]
    public partial struct DestroySystem : ISystem
    {
        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            var ecb = SystemAPI.GetSingleton<EndSimulationEntityCommandBufferSystem.Singleton>().CreateCommandBuffer(state.WorldUnmanaged);

            foreach ((_, var entity) in SystemAPI.Query<RefRO<DestroyEvent>>().WithEntityAccess())
                ecb.DestroyEntity(entity);

            foreach ((_, var entity) in SystemAPI.Query<RefRO<CreateEvent>>().WithEntityAccess())
                ecb.RemoveComponent<CreateEvent>(entity);
        }
    }
}