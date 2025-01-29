using UnityEngine;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using Unity.Burst;
using Unity.Collections;
using Game.Utility;
using Unity.Rendering;

namespace Game.ECS
{
    /// <summary>
    /// TODO: config input
    /// </summary>
    [BurstCompile]
    [UpdateAfter(typeof(GameCameraSystem))]
    public partial struct PlayerControllerSystem : ISystem, ISystemStartStop
    {
        [BurstCompile]
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<PlayerController>();
            state.RequireForUpdate<GameCamera>();
            state.RequireForUpdate<CubeGroup>();
        }

        [BurstCompile]
        public void OnStartRunning(ref SystemState state)
        {
            var player = SystemAPI.GetSingleton<PlayerController>();
            EnabledSelect(ref state, player.select, false);
        }

        //[BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            InputBuildUpdate(ref state);
        }

        //[BurstCompile]
        private void InputBuildUpdate(ref SystemState state)
        {
            var player = SystemAPI.GetSingleton<PlayerController>();

            GetRay(ref state, out float3 origin, out float3 direction);
            direction *= 100;///TODO: to config

            RaycastGroup(ref state, origin, ref direction, out Entity entityGroup, out int3 hit, out int3 normal, out bool isCollision);

            if (!isCollision)
            {
                EnabledSelect(ref state, player.select, false);
                return;
            }

            GetBuild(ref state, out var build);
            bool isHasBuild = build.IsNull == false;
            bool isCanBuild = false;

            int3 pos = hit;

            var group = SystemAPI.GetComponent<CubeGroup>(entityGroup);

            if (isHasBuild)
            {
                if (SystemAPI.HasComponent<CubeOption>(build.entity))
                {
                    pos = hit + normal;

                    var option = SystemAPI.GetComponent<CubeOption>(build.entity);
                    CanAttach(ref state, group, option, pos, normal, out bool isCanAttach);
                    isCanBuild = isCanAttach;

                    if (Input.GetMouseButtonDown(0))
                    {
                        if (isCanAttach)
                        {
                            var buffer = GetEntityCommandBuffer(ref state);

                            var entity = buffer.Instantiate(build.entity);
                            CubeUtility.ToIndex(pos, group.size, out int index);
                            buffer.AddComponent(entity, new CreateEvent());
                            buffer.AddComponent<Cube>(entity);
                            buffer.SetComponent(entity, new Cube()
                            {
                                index = index,
                                position = pos,
                                normal = normal,
                            });
                            buffer.AddComponent<Parent>(entity);
                            buffer.SetComponent(entity, new Parent() { Value = entityGroup });

                            if (EnumUtility.HasFlagUnsafe(option.flags, FlagCubeOption.Rotation))
                            {
                                var transform = SystemAPI.GetComponent<LocalToWorld>(entityGroup);
                                CubeUtility.ToLocal(ref origin, ref direction, transform.Position, transform.Rotation);
                                MathUtility.ProjectOnPlane(direction, normal, out direction);
                                CubeUtility.SingleNormalize(ref direction);
                                buffer.SetComponent(entity, LocalTransform.FromRotation(quaternion.LookRotation(direction, normal)));
                            }
                        }
                    }
                }
            }
            else
            {
                CanDetach(ref state, group, hit, out bool isDetach);
                isCanBuild = isDetach;

                if (Input.GetMouseButtonDown(0))
                {
                    if (isDetach)
                    {
                        group.GetEntity(hit, out Entity en);

                        var singleton = SystemAPI.GetSingleton<BeginSimulationEntityCommandBufferSystem.Singleton>();
                        var ecb = singleton.CreateCommandBuffer(state.WorldUnmanaged);
                        ecb.AddComponent<DestroyEvent>(en);
                    }
                }
            }

            
            if (isHasBuild)
            {
                EnabledSelect(ref state, player.select, isCanBuild);
                BuildSelect(ref state, player.select, isCanBuild);
                MoveSelect(ref state, player.select, entityGroup, pos);
            }
            else
            {
                EnabledSelect(ref state, player.select, true);
                BuildSelect(ref state, player.select, isCanBuild);
                MoveSelect(ref state, player.select, entityGroup, pos);
            }
        }

        [BurstCompile]
        private void CanAttach(ref SystemState state, in CubeGroup group, in CubeOption option, in int3 position, in int3 normal, out bool res)
        {
            group.GetIndex(position, out int index);

            //group.IsRange(index, out res);
            if (index < 0)
            {
                res = false;
                return;
            }

            group.HasCube(position, out res);
            if (res)
            {
                res = false;
                return;
            }

            group.GetIndex(position - normal, out index);

            //group.IsRange(index, out res);
            if (index < 0)
            {
                res = false;
                return;
            }

            group.HasCube(index, out res);
            if (!res)
                return;

            group.GetEntity(index, out Entity bottom);
            var opt = SystemAPI.GetComponent<CubeOption>(bottom);

            EnumUtility.HasFlagUnsafe(opt.flags, FlagCubeOption.IgnoreOtherAttach, out res);
            if (res)
            {
                res = false;
                return;
            }

            res = true;
        }

        [BurstCompile]
        private void CanDetach(ref SystemState state, in CubeGroup group, in int3 position, out bool res)
        {
            CubeUtility.ToIndex(position, group.size, out int index);
            if (index < 0)
            {
                res = true;
                return;
            }

            group.GetEntity(index, out Entity entity);
            if (SystemAPI.HasComponent<CubeOption>(entity) == false)
            {
                res = true;
                return;
            }

            if (SystemAPI.HasComponent<Cube>(entity) == false)
            {
                res = false;
                return;
            }

            var option = SystemAPI.GetComponent<CubeOption>(entity);
            EnumUtility.HasFlagUnsafe(option.flags, FlagCubeOption.HasParent, out res);
            if (res)
            {
                res = true;
            }
            else
            {
                for (int i = 0; i < 6; i++)
                {
                    int3 p = position + CubeUtility.DIR[i];
                    group.HasEntity(p, out bool isCol);
                    if (isCol)
                    {
                        group.GetEntity(p, out var e);
                        var o = SystemAPI.GetComponent<CubeOption>(e);
                        var c = SystemAPI.GetComponent<Cube>(e);
                        EnumUtility.HasFlagUnsafe(o.flags, FlagCubeOption.HasParent, out bool r);
                        if (r && (c.normal.Equals(CubeUtility.DIR[i])))
                        {
                            res = false;
                            return;
                        }
                    }
                }

                res = true;
            }
        }

        [BurstCompile]
        private EntityCommandBuffer GetEntityCommandBuffer(ref SystemState state)
        {
            var ecbSingleton = SystemAPI.GetSingleton<BeginSimulationEntityCommandBufferSystem.Singleton>();
            return ecbSingleton.CreateCommandBuffer(state.WorldUnmanaged);
        }

        //[BurstDiscard]
        private void GetBuild(ref SystemState state, out Build res)
        {
            var builds = SystemAPI.GetSingletonBuffer<Build>();
            foreach (var build in builds)
            {
                if (Game.UI.BoolStatesUIMono.Instance.GetStateValue(build.uidClick))
                {
                    res = build;
                    return;
                }
            }
            res = default;
        }

        [BurstCompile]
        private void GetRay(ref SystemState state, out float3 origin, out float3 direction)
        {
            var camera = SystemAPI.GetSingleton<GameCamera>();
            origin = camera.origin;
            direction = camera.direction;
        }

        ///TODO: raycast all group
        [BurstCompile]
        private void RaycastGroup(ref SystemState state, float3 origin, ref float3 direction, out Entity entityGroup, out int3 hit, out int3 normal, out bool isCollision)
        {
            entityGroup = SystemAPI.GetSingletonEntity<CubeGroup>();
            var group = SystemAPI.GetComponent<CubeGroup>(entityGroup);
            var transformGroup = SystemAPI.GetComponent<LocalToWorld>(entityGroup);

            if (group.IsEmpty == false)
            {
                group.Raycast(transformGroup.Position, transformGroup.Rotation,
                    origin, direction, out hit, out normal, out isCollision, true);

                if(isCollision)
                    group.GetNotFullCollision(ref hit, normal);
            }
            else
            {
                hit = 0;
                normal = 0;
                isCollision = false;
            }
        }

        [BurstCompile]
        private void MoveSelect(ref SystemState state, Entity select, Entity group, float3 position)
        {
            var tr = SystemAPI.GetComponent<LocalToWorld>(group);
            SystemAPI.SetComponent(select, LocalTransform.FromPositionRotation(tr.Position + math.mul(tr.Rotation, position + 0.5f), tr.Rotation));
        }

        [BurstCompile]
        private void EnabledSelect(ref SystemState state, Entity select, bool enabled)
        {
            bool res = SystemAPI.IsComponentEnabled<MaterialMeshInfo>(select);//DisableRendering
            if (res != enabled)
            {
                SystemAPI.SetComponentEnabled<MaterialMeshInfo>(select, enabled);
            }
        }

        [BurstCompile]
        private void BuildSelect(ref SystemState state, Entity select, bool enabled)
        {
            bool res = SystemAPI.HasComponent<URPMaterialPropertyBaseColor>(select);//DisableRendering
            if (res)
            {
                var color = SystemAPI.GetComponentRW<URPMaterialPropertyBaseColor>(select);
                color.ValueRW.Value = enabled ? (Vector4)Color.green : (Vector4)Color.red;
            }
        }

        [BurstCompile]
        public void OnStopRunning(ref SystemState state) { }
    }
}
