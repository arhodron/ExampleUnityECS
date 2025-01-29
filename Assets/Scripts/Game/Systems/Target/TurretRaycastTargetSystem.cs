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
    [UpdateInGroup(typeof(FindTargetSystemGroup))]
    [UpdateAfter(typeof(TurretAngleTargetSystem))]
    public partial struct TurretRaycastTargetSystem : ISystem
    {
        private EntityQuery targetFinderQuery;
        private EntityQuery targetValueQuery;

        private ComponentLookup<LocalToWorld> localToWorldLookup;
        private ComponentLookup<CubeGroup> cubeGroupLookup;

        [BurstCompile]
        public void OnCreate(ref SystemState state)
        {
            var builder = new EntityQueryBuilder(Allocator.Temp)
                .WithAllRW<TargetFinder>()
                .WithAll<Cube, TurretRotation, Parent>();
            targetFinderQuery = state.GetEntityQuery(builder);

            builder = new EntityQueryBuilder(Allocator.Temp)
                .WithAllRW<TargetValue>()
                .WithAll<Cube, TurretRotation, LocalToWorld, Parent>();
            targetValueQuery = state.GetEntityQuery(builder);

            localToWorldLookup = SystemAPI.GetComponentLookup<LocalToWorld>(true);
            cubeGroupLookup = SystemAPI.GetComponentLookup<CubeGroup>(true);
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            localToWorldLookup.Update(ref state);
            cubeGroupLookup.Update(ref state);

            FindTargets(ref state);

            CheckTargets(ref state);
        }

        [BurstCompile]
        private void FindTargets(ref SystemState state)
        {
            if (targetFinderQuery.IsEmpty)
                return;

            ///TODO: check all group
            var job = new FindTargetJob
            {
                localToWorldLookup = localToWorldLookup,
                cubeGroupLookup = cubeGroupLookup,
            };
            job.ScheduleParallel(targetFinderQuery, state.Dependency).Complete();
        }

        [BurstCompile]
        private void CheckTargets(ref SystemState state)
        {
            if (targetValueQuery.IsEmpty)
                return;

            ///TODO: check all group
            var job = new CheckTargetJob
            {
                localToWorldLookup = localToWorldLookup,
                cubeGroupLookup = cubeGroupLookup,
            };
            job.ScheduleParallel(targetValueQuery, state.Dependency).Complete();
        }

        [BurstCompile]
        public partial struct FindTargetJob : IJobEntity
        {
            [ReadOnly]
            public ComponentLookup<LocalToWorld> localToWorldLookup;
            [ReadOnly]
            public ComponentLookup<CubeGroup> cubeGroupLookup;

            [BurstCompile]
            public void Execute(ref TargetFinder finder, in Cube cube, in TurretRotation turret, EnabledRefRW<TargetFinder> enabled, in Parent parent)
            {
                var cache = finder.cache;
                if (cache.IsCreated == false || cache.IsEmpty)
                {
                    enabled.ValueRW = false;
                    return;
                }

                if (cubeGroupLookup.HasComponent(parent.Value) == false)
                    return;

                float3 position = localToWorldLookup[turret.vertical].Position;
                var group = cubeGroupLookup[parent.Value];
                var trGroup = localToWorldLookup[parent.Value];
                float3 posGroup = trGroup.Position;
                quaternion rotGroup = trGroup.Rotation;

                for (int count = cache.Length, i = count - 1; i >= 0; i--)
                {
                    float3 target = cache[i].position;
                    float3 direction = position - target;
                    group.Raycast(posGroup, rotGroup, target, direction, out int3 hit, out _, out bool isHit, false);
                    if (isHit && hit.Equals(cube.position) == false)
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
            [ReadOnly]
            public ComponentLookup<CubeGroup> cubeGroupLookup;

            [BurstCompile]
            public void Execute(ref TargetValue value, in Cube cube, in TurretRotation turret, EnabledRefRW<TargetValue> enabled, in LocalToWorld localToWorld, in Parent parent)
            {
                if (cubeGroupLookup.HasComponent(parent.Value) == false)
                    return;

                float3 position = localToWorldLookup[turret.vertical].Position;
                var group = cubeGroupLookup[parent.Value];
                var trGroup = localToWorldLookup[parent.Value];
                float3 posGroup = trGroup.Position;
                quaternion rotGroup = trGroup.Rotation;

                float3 target = value.position;
                float3 direction = position - target;
                group.Raycast(posGroup, rotGroup, target, direction, out int3 hit, out _, out bool isHit, false);

                if (isHit && hit.Equals(cube.position) == false)
                {
                    value.value = Entity.Null;
                    enabled.ValueRW = false;
                }
            }
        }
    }
}