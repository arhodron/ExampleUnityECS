using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using Unity.Burst;
using Random = Unity.Mathematics.Random;
using Game.ECS.SimpleCollision;

namespace Game.ECS
{
    [BurstCompile]
    [RequireMatchingQueriesForUpdate]
    [UpdateInGroup(typeof(DamageSystemGroup))]
    public partial struct DealDamageHitSystem : ISystem
    {
        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            foreach ((var damage, var hit) in SystemAPI.Query<RefRO<DamageValue>, RefRO<Hit>>().WithAll<DealDamageHit>())
            {
                var target = hit.ValueRO.value;
                if (SystemAPI.HasBuffer<TakeDamage>(target) == false)
                    continue;

                var buffer = SystemAPI.GetBuffer<TakeDamage>(target);
                buffer.Add(new TakeDamage
                {
                    value = damage.ValueRO.value
                });
            }
        }
    }
}