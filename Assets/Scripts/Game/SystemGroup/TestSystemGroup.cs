using Unity.Entities;
using Unity.Transforms;

namespace Game.ECS
{
    [UpdateBefore(typeof(TransformSystemGroup))]
    [UpdateInGroup(typeof(SimulationSystemGroup))]
    public partial class TestSystemGroup : ComponentSystemGroup { }
}