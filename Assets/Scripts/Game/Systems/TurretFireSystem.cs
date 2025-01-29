using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using Unity.Burst;
using Unity.Collections;
using Game.Utility;
using Unity.Jobs;

namespace Game.ECS
{
    [BurstCompile]
    [UpdateAfter(typeof(TargetSystemGroup))]
    [RequireMatchingQueriesForUpdate]
    [UpdateInGroup(typeof(SimulationSystemGroup))]
    public partial struct TurretFireSystem : ISystem
    {
        private EntityQuery turretFireRateQuery;

        private ComponentLookup<LocalTransform> localTransformLookup;
        private ComponentLookup<LocalToWorld> localToWorldsLookup;

        [BurstCompile]
        public void OnCreate(ref SystemState state)
        {
            turretFireRateQuery = new EntityQueryBuilder(Allocator.Temp)
                .WithAllRW<TurretFire>()
                .WithAll<TargetValue>()
                .Build(ref state);

            localTransformLookup = SystemAPI.GetComponentLookup<LocalTransform>(true);
            localToWorldsLookup = SystemAPI.GetComponentLookup<LocalToWorld>(true);
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            localTransformLookup.Update(ref state);
            localToWorldsLookup.Update(ref state);

            double time = SystemAPI.Time.ElapsedTime;

            var ecb = SystemAPI.GetSingleton<BeginSimulationEntityCommandBufferSystem.Singleton>().CreateCommandBuffer(state.WorldUnmanaged).AsParallelWriter();

            var job = new TurretFireJobEntity
            {
                time = time,
                ecb = ecb,
                localTransformLookup = localTransformLookup,
                localToWorldsLookup = localToWorldsLookup,
            };
            job.ScheduleParallel(turretFireRateQuery, state.Dependency).Complete();
        }

        [BurstCompile]
        public partial struct TurretFireJobEntity : IJobEntity
        {
            public double time;

            public EntityCommandBuffer.ParallelWriter ecb;

            [ReadOnly]
            public ComponentLookup<LocalTransform> localTransformLookup;
            [ReadOnly]
            public ComponentLookup<LocalToWorld> localToWorldsLookup;

            [BurstCompile]
            public void Execute([EntityIndexInQuery] int index, ref TurretFire fire, in TargetValue target)
            {
                if (fire.fireRate == 0)
                    return;

                float fireTime = 1 / fire.fireRate;

                if (fire.fireTime + fireTime > time)
                    return;

                LocalToWorld pivot = localToWorldsLookup[fire.pivot];
                float3 position = pivot.Position;
                float3 direction = pivot.Forward;

                MathUtility.Angle(direction, target.position - position, out float angle);
                if (angle * math.TODEGREES > 5)///TO CONST
                    return;

                fire.fireTime = time;

                quaternion rotation = quaternion.LookRotation(direction, pivot.Up);

                var projectile = ecb.Instantiate(index, fire.projectile);
                ecb.AddComponent(index, projectile, new CreateEvent());
                ecb.AddComponent(index, projectile, new TargetValue
                {
                    value = target.value,
                    position = target.position,
                });
                ecb.SetComponent(index, projectile, new LocalTransform
                {
                    Position = position,
                    Rotation = rotation,
                    Scale = localTransformLookup[fire.projectile].Scale,
                });
            }
        }
    }
}
