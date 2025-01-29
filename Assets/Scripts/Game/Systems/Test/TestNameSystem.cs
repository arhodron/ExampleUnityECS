using UnityEngine;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using Unity.Burst;
using Unity.Collections;

namespace Game.ECS.Test
{
    //[BurstCompile]
    public partial struct TestNameSystem : ISystem
    {
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<TestName>();
        }

        //[BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            var ecb = new EntityCommandBuffer(Allocator.Temp);

            foreach (var (name, entity) in
                 SystemAPI.Query<RefRO<TestName>>().WithEntityAccess())
            {
                state.EntityManager.SetName(entity, name.ValueRO.name);
                ecb.RemoveComponent<TestName>(entity);
            }

            ecb.Playback(state.EntityManager);
            ecb.Dispose();
        }

        public void OnDestroy(ref SystemState state) { }
    }
}