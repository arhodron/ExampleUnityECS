using UnityEngine;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Collections;
using Unity.Transforms;
using Unity.Burst;
using Unity.Jobs;

namespace Game.ECS
{
    [RequireMatchingQueriesForUpdate]
    [BurstCompile]
    [UpdateInGroup(typeof(TargetSystemGroup))]
    [UpdateBefore(typeof(FindTargetSystemGroup))]
    public partial struct BeforeTargetSystem : ISystem
    {
        private EntityQuery targetFinderQuery;
        private EntityQuery targetValueQuery;

        private ComponentLookup<Target> targetLookup;
        private ComponentLookup<LocalToWorld> localToWorldLookup;

        [BurstCompile]
        public void OnCreate(ref SystemState state)
        {
            var builder = new EntityQueryBuilder(Allocator.Temp)
                .WithPresent<TargetFinder, TargetValue>();
            targetFinderQuery = state.GetEntityQuery(builder);

            builder = new EntityQueryBuilder(Allocator.Temp)
                .WithPresent<TargetValue>();
            targetValueQuery = state.GetEntityQuery(builder);

            targetLookup = SystemAPI.GetComponentLookup<Target>(false);
            localToWorldLookup = SystemAPI.GetComponentLookup<LocalToWorld>(true);
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            if (targetValueQuery.IsEmpty == false)
            {
                targetLookup.Update(ref state);
                localToWorldLookup.Update(ref state);

                var job = new CheckTargetJob
                {
                    targetLookup = targetLookup,
                    localToWorldLookup = localToWorldLookup,
                };
                job.ScheduleParallel(targetValueQuery, state.Dependency).Complete();
            }

            if(targetFinderQuery.IsEmpty == false)
            {
                float deltaTime = SystemAPI.Time.DeltaTime;
                var job = new CheckFinderJob
                {
                    deltaTime = deltaTime,
                };
                job.ScheduleParallel(targetFinderQuery, state.Dependency).Complete();
            }
        }

        [BurstCompile]
        public partial struct CheckTargetJob : IJobEntity
        {
            [ReadOnly]
            public ComponentLookup<Target> targetLookup;
            [ReadOnly]
            public ComponentLookup<LocalToWorld> localToWorldLookup;

            [BurstCompile]
            public void Execute(ref TargetValue value, EnabledRefRW<TargetValue> enabled)
            {
                if (enabled.ValueRO)
                {
                    if (value.value == Entity.Null || targetLookup.EntityExists(value.value) == false)
                    {
                        value.value = Entity.Null;
                        enabled.ValueRW = false;
                    }
                    else
                    {
                        value.position = localToWorldLookup[value.value].Position;
                    }
                }
            }
        }

        [BurstCompile]
        public partial struct CheckFinderJob : IJobEntity
        {
            public float deltaTime;

            [BurstCompile]
            public void Execute(in TargetValue value, ref TargetFinder finder, EnabledRefRW<TargetFinder> enabledFinder)
            {
                bool enabled = enabledFinder.ValueRO;
                finder.state = enabled;
                enabledFinder.ValueRW = enabled && (finder.UpdateTimeRate(deltaTime) || value.value == Entity.Null);
            }
        }
    }
}