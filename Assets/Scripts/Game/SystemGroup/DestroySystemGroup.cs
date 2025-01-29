using Unity.Entities;

namespace Game.ECS
{
    [System.Obsolete]
    [UpdateBefore(typeof(EndSimulationEntityCommandBufferSystem))]
    [UpdateInGroup(typeof(SimulationSystemGroup), OrderLast = true)]
    public partial class DestroySystemGroup : ComponentSystemGroup { }
}