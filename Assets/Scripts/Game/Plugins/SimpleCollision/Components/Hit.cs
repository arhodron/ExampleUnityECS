using Unity.Entities;

namespace Game.ECS.SimpleCollision
{
    public struct Hit : IComponentData, IEnableableComponent
    {
        public Entity value;
    }
}
