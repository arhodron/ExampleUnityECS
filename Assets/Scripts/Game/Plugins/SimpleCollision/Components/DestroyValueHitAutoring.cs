using Unity.Entities;
using AutoAuthoring;
using System;

namespace Game.ECS.SimpleCollision
{
    public class DestroyValueHitAutoring : AutoAuthoring<DestroyValueHit> { }

    [Serializable]
    public struct DestroyValueHit : IComponentData { }
}