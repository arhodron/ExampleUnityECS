using Unity.Entities;

namespace Game.ECS
{
    [System.Obsolete]
    [UpdateAfter(typeof(BeginSimulationEntityCommandBufferSystem))]
    [UpdateInGroup(typeof(SimulationSystemGroup), OrderFirst = true)]
    public partial class CreateSystemGroup : ComponentSystemGroup { }
}