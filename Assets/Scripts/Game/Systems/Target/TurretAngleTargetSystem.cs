using Unity.Entities;
using Unity.Mathematics;
using Unity.Collections;
using Unity.Transforms;
using Unity.Burst;
using Unity.Jobs;
using Game.Utility;

namespace Game.ECS
{
    [RequireMatchingQueriesForUpdate]
    [BurstCompile]
    [UpdateInGroup(typeof(FindTargetSystemGroup))]
    [UpdateAfter(typeof(EnemyTargetSystem))]
    public partial struct TurretAngleTargetSystem : ISystem
    {
        private EntityQuery targetFinderQuery;
        private EntityQuery targetValueQuery;

        private ComponentLookup<LocalToWorld> localToWorldLookup;

        [BurstCompile]
        public void OnCreate(ref SystemState state)
        {
            var builder = new EntityQueryBuilder(Allocator.Temp)
                .WithAllRW<TargetFinder>()
                .WithAll<LocalToWorld>();
            targetFinderQuery = state.GetEntityQuery(builder);

            builder = new EntityQueryBuilder(Allocator.Temp)
                .WithAllRW<TargetValue>()
                .WithAll<LocalToWorld>();
            targetValueQuery = state.GetEntityQuery(builder);

            localToWorldLookup = SystemAPI.GetComponentLookup<LocalToWorld>(true);
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            localToWorldLookup.Update(ref state);

            FindTargets(ref state);

            CheckTargets(ref state);
        }

        [BurstCompile]
        private void FindTargets(ref SystemState state)
        {
            if (targetFinderQuery.IsEmpty)
                return;

            var job = new FindTargetJob
            {
                localToWorldLookup = localToWorldLookup,
            };
            job.ScheduleParallel(targetFinderQuery, state.Dependency).Complete();
        }

        [BurstCompile]
        private void CheckTargets(ref SystemState state)
        {
            if (targetValueQuery.IsEmpty)
                return;

            var job = new CheckTargetJob
            {
                localToWorldLookup = localToWorldLookup,
            };
            job.ScheduleParallel(targetValueQuery, state.Dependency).Complete();
        }

        [BurstCompile]
        public partial struct FindTargetJob : IJobEntity
        {
            [ReadOnly]
            public ComponentLookup<LocalToWorld> localToWorldLookup;

            [BurstCompile]
            public void Execute(ref TargetFinder finder, EnabledRefRW<TargetFinder> enabled, in LocalToWorld localToWorld)
            {
                var cache = finder.cache;
                if (cache.IsCreated == false || cache.IsEmpty)
                {
                    enabled.ValueRW = false;
                    return;
                }

                float3 position = localToWorld.Position;
                float3 up = localToWorld.Up;

                for (int count = cache.Length, i = count - 1; i >= 0; i--)
                {
                    float3 target = cache[i].position;
                    float3 direction = target - position;
                    MathUtility.Angle(up, direction, out float angle);
                    if (angle * math.TODEGREES > 90)
                        cache.RemoveAtSwapBack(i);
                }

                finder.cache = cache;

                if (cache.IsEmpty)
                    enabled.ValueRW = false;
            }
        }

        [BurstCompile]
        public partial struct CheckTargetJob : IJobEntity
        {
            [ReadOnly]
            public ComponentLookup<LocalToWorld> localToWorldLookup;

            [BurstCompile]
            public void Execute(ref TargetValue value, EnabledRefRW<TargetValue> enabled, in LocalToWorld localToWorld)
            {
                float3 position = localToWorld.Position;
                float3 up = localToWorld.Up;
                float3 target = localToWorldLookup[value.value].Position;
                float3 direction = target - position;
                MathUtility.Angle(up, direction, out float angle);
                if (angle * math.TODEGREES > 90)
                {
                    value.value = Entity.Null;
                    enabled.ValueRW = false;
                }
            }
        }
    }
}