using Unity.Entities;
using Unity.Mathematics;
using Unity.Collections;
using Unity.Transforms;
using Unity.Burst;
using Unity.Jobs;
using System.Collections.Generic;
using System;
using Unity.Collections.LowLevel.Unsafe;

namespace Game.ECS
{
    [RequireMatchingQueriesForUpdate]
    [BurstCompile]
    [UpdateInGroup(typeof(FindTargetSystemGroup))]
    [UpdateAfter(typeof(EnemyTargetSystem))]
    public partial struct RangeTargetSystem : ISystem
    {
        private EntityQuery targetQuery;
        private EntityQuery targetFinderQuery;
        private EntityQuery targetValueQuery;

        private ComponentLookup<Target> targetLookup;
        private ComponentLookup<LocalToWorld> localToWorldLookup;

        [BurstCompile]
        public void OnCreate(ref SystemState state)
        {
            var builder = new EntityQueryBuilder(Allocator.Temp)
                .WithAll<Target, LocalToWorld>();
            targetQuery = state.GetEntityQuery(builder);

            builder = new EntityQueryBuilder(Allocator.Temp)
                .WithAllRW<TargetFinder>()
                .WithAll<TargetRange, LocalToWorld>();
            targetFinderQuery = state.GetEntityQuery(builder);

            builder = new EntityQueryBuilder(Allocator.Temp)
                .WithAllRW<TargetValue>()
                .WithAll<TargetRange, LocalToWorld>();
            targetValueQuery = state.GetEntityQuery(builder);

            targetLookup = SystemAPI.GetComponentLookup<Target>(true);
            localToWorldLookup = SystemAPI.GetComponentLookup<LocalToWorld>(true);
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            localToWorldLookup.Update(ref state);

            FindTargets(ref state);

            CheckTarget(ref state);
        }

        [BurstCompile]
        private void FindTargets(ref SystemState state)
        {
            if (targetQuery.IsEmpty || targetFinderQuery.IsEmpty)
                return;

            var targets = targetQuery.ToEntityArray(state.WorldUnmanaged.UpdateAllocator.ToAllocator);

            var job = new FindRangeTargetJob
            {
                targets = targets,
                localToWorldLookup = localToWorldLookup,
            };
            job.ScheduleParallel(targetFinderQuery, state.Dependency).Complete();

            targets.Dispose();
        }

        [BurstCompile]
        private void CheckTarget(ref SystemState state)
        {
            if (targetValueQuery.IsEmpty)
                return;

            localToWorldLookup.Update(ref state);
            targetLookup.Update(ref state);

            var job = new CheckRangeTargetJob
            {
                targetLookup = targetLookup,
            };
            job.ScheduleParallel(targetValueQuery, state.Dependency).Complete();
        }

        [BurstCompile]
        public partial struct FindRangeTargetJob : IJobEntity
        {
            [ReadOnly]
            public NativeArray<Entity> targets;
            [ReadOnly]
            public ComponentLookup<LocalToWorld> localToWorldLookup;

            [BurstCompile]
            public void Execute(ref TargetFinder finder, EnabledRefRW<TargetFinder> enabled, in TargetRange range, in LocalToWorld localToWorld)
            {
                float3 position = localToWorld.Position;
                float distance = range.value;

                UnsafeList<TargetPosition> cache;
                NativeList<Distance> distances;
                if (finder.cache.IsCreated)
                {
                    cache = finder.cache;
                    distances = new NativeList<Distance>(cache.Length, Allocator.TempJob);

                    for (int count = cache.Length, i = count - 1; i >= 0; i--)
                    {
                        TargetPosition target = cache[i];
                        float3 pos = cache[i].position;
                        float dist = math.distance(position, pos);
                        if (dist < distance)
                            distances.Add(new Distance
                            {
                                entity = target.value,
                                position = pos,
                                distance = dist,
                            });
                        else
                            cache.RemoveAtSwapBack(i);
                    }

                    distances.Sort(new CompareDistance());

                    for (int i = 0, count = distances.Length; i < count; i++)
                        cache[i] = new TargetPosition(distances[i].entity, distances[i].position);
                }
                else
                {
                    distances = new NativeList<Distance>(targets.Length, Allocator.TempJob);

                    for (int i = 0; i < targets.Length; i++)
                    {
                        var target = targets[i];
                        float3 pos = localToWorldLookup[target].Position;
                        float dist = math.distance(position, pos);
                        if (dist < distance)
                        {
                            distances.Add(new Distance
                            {
                                entity = target,
                                distance = dist,
                                position = pos,
                            });
                        }
                    }

                    distances.Sort(new CompareDistance());

                    cache = new UnsafeList<TargetPosition>(distances.Length, Allocator.TempJob);

                    for (int i = 0, count = distances.Length; i < count; i++)
                        cache.Add(new TargetPosition(distances[i].entity, distances[i].position));
                }

                if (cache.IsEmpty)
                {
                    enabled.ValueRW = false;
                }

                distances.Dispose();

                finder.cache = cache;
            }

            [BurstCompile]
            private struct Distance : IComparable<Distance>
            {
                public Entity entity;
                public float3 position;
                public float distance;

                [BurstCompile]
                public int CompareTo(Distance other)
                {
                    return distance.CompareTo(other);
                }
            }

            private struct CompareDistance : IComparer<Distance>
            {
                public int Compare(Distance x, Distance y)
                {
                    return x.distance.CompareTo(y.distance);
                }
            }
        }

        [BurstCompile]
        public partial struct CheckRangeTargetJob : IJobEntity
        {
            [ReadOnly]
            public ComponentLookup<Target> targetLookup;

            [BurstCompile]
            public void Execute(ref TargetValue value, EnabledRefRW<TargetValue> enabled, in TargetRange range, in LocalToWorld localToWorld)
            {
                float3 position = localToWorld.Position;
                float3 target = value.position;
                if (math.distance(position, target) > range.value)
                    enabled.ValueRW = false;
            }
        }
    }
}
