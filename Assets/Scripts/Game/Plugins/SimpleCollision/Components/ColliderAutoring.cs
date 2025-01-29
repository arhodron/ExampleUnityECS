using Unity.Entities;
using AutoAuthoring;
using System;

namespace Game.ECS.SimpleCollision
{
    public class ColliderAutoring : AutoAuthoring<Collider> { }

    [Serializable]
    public struct Collider : IComponentData
    {
        public float radius;
    }
}
