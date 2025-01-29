using Unity.Entities;
using Unity.Burst;

namespace Game.ECS.SimpleCollision
{
    [RequireMatchingQueriesForUpdate]
    [BurstCompile]
    [UpdateInGroup(typeof(CollisionSystemGroup))]
    public partial struct DestroyThisHitSystem : ISystem
    {
        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            var ecb = SystemAPI.GetSingleton<BeginSimulationEntityCommandBufferSystem.Singleton>().CreateCommandBuffer(state.WorldUnmanaged);

            foreach ((_, var entity) in SystemAPI.Query<RefRO<Hit>>().WithAll<DestroyThisHit>().WithNone<DestroyEvent>().WithEntityAccess())
            {
                ecb.AddComponent(entity, new DestroyEvent());
            }
        }
    }
}