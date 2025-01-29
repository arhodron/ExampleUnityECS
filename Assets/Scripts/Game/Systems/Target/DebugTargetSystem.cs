using UnityEngine;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

namespace Game.ECS
{
    [UpdateInGroup(typeof(TargetSystemGroup))]
    [UpdateAfter(typeof(AfterTargetSystem))]
    public partial struct DebugTargetSystem : ISystem
    {
        private ComponentLookup<LocalTransform> localTransformLookup;
        private ComponentLookup<LocalToWorld> localToWorldLookup;

        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<TargetValue>();
        }

        public void OnUpdate(ref SystemState state)
        {
            foreach ((var target, var entity) in SystemAPI.Query<RefRO<TargetValue>>().WithEntityAccess())
            {
                if (SystemAPI.Exists(target.ValueRO.value) == false)
                    continue;

                float3 start = SystemAPI.GetComponent<LocalToWorld>(entity).Position;
                float3 end = target.ValueRO.position;
                Debug.DrawLine(start, end);
            }
        }
    }
}
