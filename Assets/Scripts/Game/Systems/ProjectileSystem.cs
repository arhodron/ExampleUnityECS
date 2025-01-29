using UnityEngine;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using Unity.Burst;
using Unity.Collections;
using Game.Utility;
using Unity.Burst.Intrinsics;
using Unity.Jobs;
using Unity.Assertions;
using Unity.Profiling;

namespace Game.ECS
{
    [BurstCompile]
    [RequireMatchingQueriesForUpdate]
    [UpdateInGroup(typeof(SimulationSystemGroup))]
    public partial struct ProjectileSystem : ISystem
    {
        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            foreach ((var target, var transform, var speed, var move) in SystemAPI.Query<RefRO<TargetValue>, RefRO<LocalTransform>, RefRO<Speed>, RefRW<Move>>().WithAll<Projectile, CreateEvent>())
            {
                float3 dir = target.ValueRO.position - transform.ValueRO.Position;
                dir = math.normalize(dir);
                move.ValueRW.direction = dir * speed.ValueRO.value;
            }
        }
    }
}