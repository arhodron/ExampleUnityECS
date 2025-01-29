using Unity.Entities;
using Unity.Transforms;
using Unity.Burst;
using Unity.Collections;
using Unity.Burst.Intrinsics;
using Unity.Assertions;

namespace Game.ECS
{
    [BurstCompile]
    [RequireMatchingQueriesForUpdate]
    public partial struct MoveSystem : ISystem
    {
        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            var spinningCubesQuery = SystemAPI.QueryBuilder().WithAll<Move, LocalTransform>().Build();

            var job = new MoveJob
            {
                transformTypeHandle = SystemAPI.GetComponentTypeHandle<LocalTransform>(),
                moveTypeHandle = SystemAPI.GetComponentTypeHandle<Move>(true),
                deltaTime = SystemAPI.Time.DeltaTime
            };

            job.Schedule(spinningCubesQuery, state.Dependency).Complete();
        }

        public void OnDestroy(ref SystemState state) { }
    }

    [BurstCompile]
    public partial struct MoveJob : IJobChunk
    {
        [NativeDisableParallelForRestriction]
        public ComponentTypeHandle<LocalTransform> transformTypeHandle;
        [ReadOnly]
        public ComponentTypeHandle<Move> moveTypeHandle;
        public float deltaTime;

        [BurstCompile]
        public void Execute(in ArchetypeChunk chunk, int unfilteredChunkIndex, bool useEnabledMask, in v128 chunkEnabledMask)
        {
            Assert.IsFalse(useEnabledMask);

            NativeArray<LocalTransform> transforms = chunk.GetNativeArray(ref transformTypeHandle);
            NativeArray<Move> moves = chunk.GetNativeArray(ref moveTypeHandle);

            for (int i = 0, chunkEntityCount = chunk.Count; i < chunkEntityCount; i++)
                transforms[i] = transforms[i].Translate(moves[i].direction * deltaTime);
        }
    }
}
