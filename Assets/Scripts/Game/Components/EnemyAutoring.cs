using Unity.Entities;
using AutoAuthoring;

namespace Game.ECS
{
    public class EnemyAutoring : AutoAuthoring<Enemy> { }

    [System.Serializable]
    public struct Enemy : IComponentData { }
}
