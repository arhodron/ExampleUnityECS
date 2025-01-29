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
    //[UpdateInGroup(typeof(SimulationSystemGroup))]
    //[UpdateAfter(typeof(FindTargetSystemGroup))]
    public partial struct TurretRotationSystem : ISystem
    {
        static readonly ProfilerMarker profileTurrets = new ProfilerMarker(nameof(TurretRotationSystem));

        private EntityQuery turretQuery;
        private ComponentTypeHandle<TurretRotation> turretHandleRO;
        private ComponentTypeHandle<LocalToWorld> rootHandleRO;

        private ComponentLookup<LocalTransform> localTransforms;
        private ComponentLookup<LocalToWorld> localToWorlds;

        [BurstCompile]
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<TurretRotation>();
            state.RequireForUpdate<GameManager>();

            var builder = new EntityQueryBuilder(Allocator.Temp)
                .WithAll<TurretRotation, TargetValue>()
                .WithOptions(EntityQueryOptions.FilterWriteGroup);
            turretQuery = state.GetEntityQuery(builder);

            turretHandleRO = state.GetComponentTypeHandle<TurretRotation>(true);
            rootHandleRO = state.GetComponentTypeHandle<LocalToWorld>(true);

            localTransforms = SystemAPI.GetComponentLookup<LocalTransform>(false);
            localToWorlds = SystemAPI.GetComponentLookup<LocalToWorld>(true);
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            profileTurrets.Begin();

            RotateTurretsEntity(ref state);

            profileTurrets.End();
        }

        [BurstCompile]
        private void RotateTurretsEntity(ref SystemState state)
        {
            if (turretQuery.IsEmptyIgnoreFilter)
                return;

            float deltaTime = SystemAPI.Time.DeltaTime;

            localTransforms.Update(ref state);
            localToWorlds.Update(ref state);

            var job = new TurretRotationJobEntity
            {
                deltaTime = deltaTime,
                localTransforms = localTransforms,
                localToWorlds = localToWorlds,
            };
            job.ScheduleParallel(state.Dependency).Complete();
        }

        [BurstCompile]
        public partial struct TurretRotationJobEntity : IJobEntity
        {
            public float deltaTime;

            [NativeDisableParallelForRestriction]
            public ComponentLookup<LocalTransform> localTransforms;
            [ReadOnly]
            public ComponentLookup<LocalToWorld> localToWorlds;

            [BurstCompile]
            public void Execute(in TurretRotation turret, in TargetValue target, in LocalToWorld root)
            {
                if (target.value == Entity.Null)
                    return;

                float3 targetPosition = target.position;
                float3 up = root.Up;
                float speedRotation = turret.speed * deltaTime * math.TORADIANS;
                

                var transformH = localTransforms[turret.horizontal];
                var worldH = localToWorlds[turret.horizontal];
                var matrixH = worldH.Value;

                TurretUtility.RotationHorizontal(targetPosition, worldH.Position, up, out var rotationH);
                rotationH = math.inverse(root.Value).TransformRotation(rotationH);
                MathUtility.RotateTowards(transformH.Rotation, rotationH, speedRotation, out rotationH);
                transformH.Rotation = rotationH;
                localTransforms[turret.horizontal] = transformH;


                var transformV = localTransforms[turret.vertical];
                //var matrixV = worldH.Value;
                var worldV = localToWorlds[turret.vertical];

                quaternion rotationV = transformV.Rotation;
                TurretUtility.RotationVertical(targetPosition, worldV.Position, matrixH, ref rotationV);//TransformHelpers.TransformPoint(matrixV, transformV.Position)
                MathUtility.RotateTowards(transformV.Rotation, rotationV, speedRotation, out rotationV);
                transformV.Rotation = rotationV;
                localTransforms[turret.vertical] = transformV;
            }
        }

        [BurstCompile]
        public struct TurretRotationJobChunk : IJobChunk
        {
            public float deltaTime;
            public float3 targetPosition;

            [ReadOnly]
            public ComponentTypeHandle<TurretRotation> turretHandle;
            [ReadOnly]
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
                    var turret = turrets[i];
                    var root = roots[i];

                    float3 up = root.Up;
                    float speedRotation = turret.speed * deltaTime * math.TORADIANS;

                    var transformH = localTransforms[turret.horizontal];
                    var worldH = worldTransforms[turret.horizontal];
                    var matrixH = worldH.Value;

                    TurretUtility.RotationHorizontal(targetPosition, worldH.Position, up, out var rotationH); //TransformHelpers.TransformPoint(matrixH, transformH.Position)
                    rotationH = math.inverse(root.Value).TransformRotation(rotationH);
                    MathUtility.RotateTowards(transformH.Rotation, rotationH, speedRotation, out rotationH);
                    transformH.Rotation = rotationH;
                    localTransforms[turret.horizontal] = transformH;


                    var transformV = localTransforms[turret.vertical];
                    //var matrixV = worldH.Value;
                    var worldV = worldTransforms[turret.vertical];

                    quaternion rotationV = transformV.Rotation;
                    TurretUtility.RotationVertical(targetPosition, worldV.Position, matrixH, ref rotationV);//TransformHelpers.TransformPoint(matrixV, transformV.Position)
                    MathUtility.RotateTowards(transformV.Rotation, rotationV, speedRotation, out rotationV);
                    transformV.Rotation = rotationV;
                    localTransforms[turret.vertical] = transformV;
                }
            }
        }
    }
}

