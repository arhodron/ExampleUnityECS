using Unity.Entities;
using Unity.Collections;
using Unity.Transforms;
using Unity.Burst;
using Unity.Jobs;
using Unity.Collections.LowLevel.Unsafe;

namespace Game.ECS
{
    [RequireMatchingQueriesForUpdate]
    [BurstCompile]
    [UpdateInGroup(typeof(FindTargetSystemGroup))]
    public partial struct EnemyTargetSystem : ISystem
    {
        private EntityQuery finderQuery;
        private EntityQuery targetQuery;

        private ComponentLookup<Enemy> enemytLookup;
        private ComponentLookup<LocalToWorld> localToWorldLookup;

        [BurstCompile]
        public void OnCreate(ref SystemState state)
        {
            var builder = new EntityQueryBuilder(Allocator.Temp)
                .WithAllRW<TargetFinder>();
            finderQuery = state.GetEntityQuery(builder);

            builder = new EntityQueryBuilder(Allocator.Temp)
                .WithAll<Target>();
            targetQuery = state.GetEntityQuery(builder);

            enemytLookup = SystemAPI.GetComponentLookup<Enemy>(true);
            localToWorldLookup = SystemAPI.GetComponentLookup<LocalToWorld>(true);
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            if (finderQuery.IsEmpty || targetQuery.IsEmpty)
                return;

            enemytLookup.Update(ref state);
            localToWorldLookup.Update(ref state);

            var targets = targetQuery.ToEntityArray(state.WorldUnmanaged.UpdateAllocator.ToAllocator);

            var job = new EnemyTargetJob
            {
                targets = targets,
                enemytLookup = enemytLookup,
                localToWorldLookup = localToWorldLookup,
            };
            job.ScheduleParallel(finderQuery, state.Dependency).Complete();

            targets.Dispose();
        }

        [BurstCompile]
        public partial struct EnemyTargetJob : IJobEntity
        {
            [ReadOnly]
            public NativeArray<Entity> targets;
            [ReadOnly]
            public ComponentLookup<Enemy> enemytLookup;
            [ReadOnly]
            public ComponentLookup<LocalToWorld> localToWorldLookup;

            [BurstCompile]
            public void Execute(in Entity entity, ref TargetFinder finder, EnabledRefRW<TargetFinder> enabled)
            {
                UnsafeList<TargetPosition> cache = finder.cache;

                if (finder.cache.IsCreated)
                {
                    bool isEnemy = enemytLookup.HasComponent(entity);

                    for (int count = cache.Length, i = count - 1; i >= 0; i--)
                        if (enemytLookup.HasComponent(cache[i].value) == isEnemy)
                            cache.RemoveAtSwapBack(i);
                }
                else
                {
                    bool isEnemy = enemytLookup.HasComponent(entity);
                    cache = new UnsafeList<TargetPosition>(targets.Length, Allocator.TempJob);

                    for (int i = 0, count = targets.Length; i < count; i++)
                        if (enemytLookup.HasComponent(targets[i]) != isEnemy)
                            cache.Add(new TargetPosition(targets[i], localToWorldLookup[targets[i]].Position));
                }

                if (cache.IsEmpty)
                {
                    enabled.ValueRW = false;
                    cache.Dispose();
                }
                else
                {
                    finder.cache = cache;
                }
            }
        }
    }
}
