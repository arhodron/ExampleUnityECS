using Unity.Entities;
using Unity.Collections;
using Unity.Burst;
using Unity.Jobs;

namespace Game.ECS
{
    [RequireMatchingQueriesForUpdate]
    [BurstCompile]
    [UpdateInGroup(typeof(TargetSystemGroup))]
    [UpdateAfter(typeof(FindTargetSystemGroup))]
    public partial struct AfterTargetSystem : ISystem
    {
        private EntityQuery targetFinderQuery;

        [BurstCompile]
        public void OnCreate(ref SystemState state)
        {
            targetFinderQuery = new EntityQueryBuilder(Allocator.Temp)
                .WithPresentRW<TargetFinder, TargetValue>()
                .Build(ref state);
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            if (targetFinderQuery.IsEmpty == false)
            {
                var job = new CompliteTargetJob();
                job.ScheduleParallel(targetFinderQuery, state.Dependency).Complete();
            }

            foreach (var finder in SystemAPI.Query<RefRW<TargetFinder> , RefRO<DestroyEvent>>())
            {
                finder.Item1.ValueRW.Dispose();
            }
        }

        public void OnDestroy(ref SystemState state)
        {
            foreach (var group in SystemAPI.Query<RefRW<TargetFinder>>().WithOptions(EntityQueryOptions.IncludeDisabledEntities))
                group.ValueRW.Dispose();
        }

        [BurstCompile]
        public partial struct CompliteTargetJob : IJobEntity
        {
            [BurstCompile]
            public void Execute(ref TargetFinder finder, EnabledRefRW<TargetFinder> enabledFinder, ref TargetValue value, EnabledRefRW<TargetValue> enabledValue)
            {
                if (finder.state)
                {
                    if (finder.cache.IsCreated)
                    {
                        if (enabledFinder.ValueRO && finder.cache.IsEmpty == false)
                        {
                            value.value = finder.cache[0].value;
                            enabledValue.ValueRW = true;
                        }
                        finder.cache.Dispose();
                    }
                    enabledFinder.ValueRW = true;
                }
            }
        }

        /*[BurstCompile]
        public struct CompliteTargetJob : IJobChunk
        {
            public ComponentTypeHandle<TurretRotation> turretHandle;
            public ComponentTypeHandle<LocalToWorld> rootHandle;

            [NativeDisableParallelForRestriction]
            public ComponentLookup<LocalTransform> localTransforms;
            [ReadOnly]
            public ComponentLookup<LocalToWorld> worldTransforms;

            [BurstCompile]
            public void Execute(in ArchetypeChunk chunk, int unfilteredChunkIndex, bool useEnabledMask,
                in v128 chunkEnabledMask)
            {
                Assert.IsFalse(useEnabledMask);

                var turrets = chunk.GetNativeArray(ref turretHandle);
                var roots = chunk.GetNativeArray(ref rootHandle);

                for (int i = 0, count = turrets.Length; i < count; i++)
                {

                }
            }
        }*/
    }
}