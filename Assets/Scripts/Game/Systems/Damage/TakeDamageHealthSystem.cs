using Unity.Entities;
using Unity.Burst;

namespace Game.ECS
{
    [BurstCompile]
    [RequireMatchingQueriesForUpdate]
    [UpdateInGroup(typeof(DamageSystemGroup))]
    [UpdateAfter(typeof(DealDamageHitSystem))]
    public partial struct TakeDamageHealthSystem : ISystem
    {
        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            var ecb = SystemAPI.GetSingleton<BeginSimulationEntityCommandBufferSystem.Singleton>().CreateCommandBuffer(state.WorldUnmanaged);

            foreach ((var health, var damage, var entity) in SystemAPI.Query<RefRW<Health>, DynamicBuffer<TakeDamage>>().WithEntityAccess())
            {
                if (damage.Length > 0)
                {
                    float value = health.ValueRO.value;
                    foreach (var dmg in damage)
                        value -= dmg.value;

                    damage.Clear();

                    health.ValueRW.value = value;

                    if (value <= 0)
                        ecb.AddComponent(entity, new DestroyEvent());
                }
                
            }
        }
    }
}