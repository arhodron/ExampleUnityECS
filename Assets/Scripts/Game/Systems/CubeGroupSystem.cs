using Unity.Entities;
using Unity.Mathematics;
using Unity.Collections;
using Unity.Transforms;
using Game.Utility;

namespace Game.ECS
{
    [RequireMatchingQueriesForUpdate]
    public partial struct CubeGroupSystem : ISystem
    {
        public void OnUpdate(ref SystemState state)
        {
            UpdateCreateEvents(ref state);

            UpdateDestroyEvents(ref state);
        }

        private void UpdateCreateEvents(ref SystemState state)
        {
            var ecb = GetEntityCommandBuffer(ref state);

            foreach (var (group, entity) in SystemAPI.Query<RefRW<CubeGroup>>().WithAll<CreateEvent>().WithEntityAccess())
            {
                group.ValueRW.Init();

                ecb.RemoveComponent<CreateEvent>(entity);

                RandomGenerateCube(ref state, entity, group.ValueRO.size, ecb);
            }

            foreach (var (cube, option, parent, transform, entity) in SystemAPI.Query<RefRO<Cube>, RefRO<CubeOption>, RefRO<Parent>, RefRW<LocalTransform>>().WithAll<CreateEvent>().WithEntityAccess())
            {
                var group = SystemAPI.GetComponentRW<CubeGroup>(parent.ValueRO.Value);
                group.ValueRW.SetEntity(cube.ValueRO.index, entity, EnumUtility.HasFlagUnsafe(option.ValueRO.flags, FlagCubeOption.FullCollision));
                transform.ValueRW = LocalTransform.FromPositionRotation((float3)cube.ValueRO.position + 0.5f + math.mul(transform.ValueRW.Rotation, option.ValueRO.offset), transform.ValueRW.Rotation);
                ecb.RemoveComponent<CreateEvent>(entity);
            }
        }

        private void RandomGenerateCube(ref SystemState state, in Entity entity, in int3 size, in EntityCommandBuffer ecb)
        {
            GetRandomIndexes(size, out var positions);

            GameManager manager = SystemAPI.GetSingleton<GameManager>();
            Entity prefab = manager.prefab;

            for (int i = 0; i < positions.Length; i++)
            {
                var ent = ecb.Instantiate(prefab);
                ecb.AddComponent(ent, new CreateEvent());
                ecb.AddComponent<Cube>(ent);
                CubeUtility.ToPosition(positions[i], size, out int3 pos);
                ecb.SetComponent(ent, new Cube()
                {
                    index = positions[i],
                    position = pos,
                });

                ecb.AddComponent(ent, new Parent() { Value = entity });
            }

            positions.Dispose();
        }

        private void GetRandomIndexes(in int3 size, out NativeList<int> res)
        {
            int max = size.x * size.y * size.z;

            var random = new Unity.Mathematics.Random(123);
            int count = max / 2;

            res = new NativeList<int>(max, Allocator.Temp);
            for (int i = 0; i < max; i++)
                res.Add(i);

            for (int i = 0; i < count; i++)
            {
                int index = random.NextInt(res.Length);
                res.RemoveAt(index);
            }
        }

        private void UpdateDestroyEvents(ref SystemState state)
        {
            foreach (var (cube, parent) in SystemAPI.Query<RefRO<Cube>, RefRO<Parent>>().WithAll<DestroyEvent>())
            {
                var group = SystemAPI.GetComponentRW<CubeGroup>(parent.ValueRO.Value);
                if (SystemAPI.HasComponent<DestroyEvent>(parent.ValueRO.Value))
                    continue;

                group.ValueRW.RemoveEntity(cube.ValueRO.index);
            }

            foreach (var group in SystemAPI.Query<RefRW<CubeGroup>>().WithAll<DestroyEvent>())
            {
                ///TODO: destroy childrens
                
                group.ValueRW.Dispose();
            }
        }

        public void OnDestroy(ref SystemState state)
        {
            foreach (var group in SystemAPI.Query<RefRW<CubeGroup>>().WithOptions(EntityQueryOptions.IncludeDisabledEntities))
                group.ValueRW.Dispose();
        }

        private EntityCommandBuffer GetEntityCommandBuffer(ref SystemState state)
        {
            var ecbSingleton = SystemAPI.GetSingleton<BeginSimulationEntityCommandBufferSystem.Singleton>();
            return ecbSingleton.CreateCommandBuffer(state.WorldUnmanaged);
        }
    }
}