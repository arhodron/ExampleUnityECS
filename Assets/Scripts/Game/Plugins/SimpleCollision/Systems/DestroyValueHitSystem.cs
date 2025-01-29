using Unity.Entities;
using Unity.Burst;

namespace Game.ECS.SimpleCollision
{
    [RequireMatchingQueriesForUpdate]
    [BurstCompile]
    [UpdateInGroup(typeof(CollisionSystemGroup))]
    public partial struct DestroyValueHitSystem : ISystem
    {
        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            var ecb = SystemAPI.GetSingleton<BeginSimulationEntityCommandBufferSystem.Singleton>().CreateCommandBuffer(state.WorldUnmanaged);

            foreach (var hit in SystemAPI.Query<RefRO<Hit>>().WithAll<DestroyValueHit>().WithNone<DestroyEvent>())
            {
                ecb.AddComponent(hit.ValueRO.value, new DestroyEvent());
            }
        }
    }
}