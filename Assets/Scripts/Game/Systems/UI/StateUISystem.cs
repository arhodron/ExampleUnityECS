using Unity.Entities;
using Game.UI;

namespace Game.ECS.UI
{
    [RequireMatchingQueriesForUpdate]
    [UpdateInGroup(typeof(UISystemGroup))]
    public partial struct StateUISystem : ISystem
    {
        public void OnUpdate(ref SystemState state)
        {
            UpdateClickState(ref state);
        }

        private void UpdateClickState(ref SystemState state)
        {
            var states = StatesUIMono<bool>.Instance;
            if (states == null)
                return;

            foreach ((var value, var enabled) in
                     SystemAPI.Query<RefRW<ClickStateUI>, EnabledRefRW<ClickStateUI>>().WithOptions(EntityQueryOptions.IgnoreComponentEnabledState))
            {
                ref var val = ref value.ValueRW;

                if (states.GetStateValue(val.uid, out var st))
                {
                    val.UpdateState(st);
                    enabled.ValueRW = val.HasState();
                }
                else
                {
                    val.Reset();
                    enabled.ValueRW = false;
                }
            }
        }
    }
}
