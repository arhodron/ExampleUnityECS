using Unity.Entities;
using Unity.Mathematics;
using Unity.Collections;
using Unity.Transforms;
using Unity.Burst;
using Unity.Jobs;
using System.Collections.Generic;
using System;
using Unity.Collections.LowLevel.Unsafe;
using Game.Utility;
using UnityEngine;

namespace Game.ECS
{
    [RequireMatchingQueriesForUpdate]
    [BurstCompile]
    [UpdateInGroup(typeof(FindTargetSystemGroup))]
    [UpdateAfter(typeof(EnemyTargetSystem))]
    public partial struct PredictiveTurretSystem : ISystem
    {
        private EntityQuery targetFinderQuery;
        private EntityQuery targetValueQuery;

        private ComponentLookup<LocalToWorld> localToWorldLookup;
        private ComponentLookup<Speed> speeedLookup;
        private ComponentLookup<Move> moveLookup;

        [BurstCompile]
        public void OnCreate(ref SystemState state)
        {
            var builder = new EntityQueryBuilder(Allocator.Temp)
                .WithAllRW<TargetFinder>()
                .WithAll<LocalToWorld, TurretFire, TurretRotation>();
            targetFinderQuery = state.GetEntityQuery(builder);

            builder = new EntityQueryBuilder(Allocator.Temp)
                .WithAllRW<TargetValue>()
                .WithAll<LocalToWorld, TurretFire, TurretRotation>();
            targetValueQuery = state.GetEntityQuery(builder);

            localToWorldLookup = SystemAPI.GetComponentLookup<LocalToWorld>(true);
            speeedLookup = SystemAPI.GetComponentLookup<Speed>(true);
            moveLookup = SystemAPI.GetComponentLookup<Move>(true);
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            localToWorldLookup.Update(ref state);
            speeedLookup.Update(ref state);
            moveLookup.Update(ref state);

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
                speeedLookup = speeedLookup,
                moveLookup = moveLookup,
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
                speeedLookup = speeedLookup,
                moveLookup = moveLookup,
            };
            job.ScheduleParallel(targetValueQuery, state.Dependency).Complete();
        }

        [BurstCompile]
        public partial struct FindTargetJob : IJobEntity
        {
            [ReadOnly]
            public ComponentLookup<LocalToWorld> localToWorldLookup;
            [ReadOnly]
            public ComponentLookup<Speed> speeedLookup;
            [ReadOnly]
            public ComponentLookup<Move> moveLookup;

            [BurstCompile]
            public void Execute(ref TargetFinder finder, EnabledRefRW<TargetFinder> enabled, in LocalToWorld localToWorld, in TurretFire turret, in TurretRotation turretRotation)
            {
                var cache = finder.cache;
                if (cache.IsCreated == false || cache.IsEmpty)
                {
                    enabled.ValueRW = false;
                    return;
                }

                var proj = turret.projectile;
                if (speeedLookup.HasComponent(proj) == false)
                    return;

                float3 position = localToWorldLookup[turretRotation.vertical].Position;//localToWorld.Position;
                float speed = speeedLookup[proj].value;

                for (int i = 0, count = cache.Length; i < count; i++)
                {
                    var target = cache[i].value;
                    if (moveLookup.HasComponent(target) == false)
                        continue;

                    float3 dir = moveLookup[target].direction;

                    TurretUtility.PredictiveProjectile(position, speed, cache[i].position, dir, out float3 pos);
                    var ch = cache[i];
                    ch.position = pos;
                    cache[i] = ch;
                }

                finder.cache = cache;
            }
        }

        [BurstCompile]
        public partial struct CheckTargetJob : IJobEntity
        {
            [ReadOnly]
            public ComponentLookup<LocalToWorld> localToWorldLookup;
            [ReadOnly]
            public ComponentLookup<Speed> speeedLookup;
            [ReadOnly]
            public ComponentLookup<Move> moveLookup;

            [BurstCompile]
            public void Execute(ref TargetValue target, in LocalToWorld localToWorld, in TurretFire turret, in TurretRotation turretRotation)
            {
                var proj = turret.projectile;
                if (speeedLookup.HasComponent(proj) == false)
                    return;

                float speed = speeedLookup[proj].value;

                if (moveLookup.HasComponent(target.value) == false)
                    return;

                float3 dir = moveLookup[target.value].direction;

                float3 position = localToWorldLookup[turretRotation.vertical].Position;//localToWorld.Position;

                TurretUtility.PredictiveProjectile(position, speed, target.position, dir, out float3 pos);
                target.position = pos;
            }
        }
    }
}