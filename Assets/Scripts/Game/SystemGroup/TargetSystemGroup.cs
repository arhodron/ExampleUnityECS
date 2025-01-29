using Unity.Entities;
using Unity.Transforms;

namespace Game.ECS
{
    [UpdateInGroup(typeof(SimulationSystemGroup))]
    [UpdateAfter(typeof(TransformSystemGroup))]
    public partial class TargetSystemGroup : ComponentSystemGroup
    {
        protected override void OnDestroy()
        {
            base.OnDestroy();
            foreach (var finder in SystemAPI.Query<RefRW<TargetFinder>>().WithOptions(EntityQueryOptions.IncludeDisabledEntities))
                finder.ValueRW.Dispose();
        }
    }
}