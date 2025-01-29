using Unity.Entities;

namespace Game.ECS.UI
{
    [UpdateInGroup(typeof(SimulationSystemGroup))]
    public partial class UISystemGroup : ComponentSystemGroup { }
}