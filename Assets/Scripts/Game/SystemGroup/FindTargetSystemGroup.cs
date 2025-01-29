using Unity.Entities;

namespace Game.ECS
{
    [UpdateInGroup(typeof(TargetSystemGroup))]
    public partial class FindTargetSystemGroup : ComponentSystemGroup { }
}