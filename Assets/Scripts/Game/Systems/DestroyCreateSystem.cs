using Unity.Entities;
using Unity.Transforms;
using Unity.Burst;

namespace Game.ECS
{
    [BurstCompile]
    [RequireMatchingQueriesForUpdate]
    [UpdateInGroup(typeof(SimulationSystemGroup))]
    public partial struct DestroyCreateSystem : ISystem
    {
        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            var ecb = SystemAPI.GetSingleton<BeginSimulationEntityCommandBufferSystem.Singleton>().CreateCommandBuffer(state.WorldUnmanaged);

            foreach ((var destroy, var transform) in SystemAPI.Query<RefRO<DestroyCreate>, RefRO<LocalToWorld>>().WithAll<DestroyEvent>())
            {
                var prefab = destroy.ValueRO.prefab;
                var res = ecb.Instantiate(prefab);
                var tr = SystemAPI.GetComponent<LocalTransform>(prefab);
                tr.Position = transform.ValueRO.Position;
                ecb.SetComponent(res, tr);
            }
        }
    }
}