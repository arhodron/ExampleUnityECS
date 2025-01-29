using Unity.Entities;
using System;

namespace Game.ECS
{
    public class ProjectileAutoring : AutoAuthoring.AutoAuthoring<Projectile> { }

    [Serializable]
    public struct Projectile : IComponentData { }
}