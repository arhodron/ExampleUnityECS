using UnityEngine;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using Unity.Burst;

namespace Game.ECS
{
    [BurstCompile]
    [RequireMatchingQueriesForUpdate]
    [UpdateInGroup(typeof(DebugSystemGroup))]
    public partial struct CubeGroupSystemDebug : ISystem
    {
        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            foreach (var (transform, group) in
                     SystemAPI.Query<RefRO<LocalTransform>, RefRO<CubeGroup>>())
            {
                float3 position = transform.ValueRO.Position;
                float3 size = group.ValueRO.size;

                Debug.DrawLine(position, position + new float3(size.x, 0, 0));
                Debug.DrawLine(position, position + new float3(0, 0, size.y));
                Debug.DrawLine(position, position + new float3(0, 0, size.y));
            }
        }
    }
}