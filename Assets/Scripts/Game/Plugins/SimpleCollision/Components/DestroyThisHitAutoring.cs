using Unity.Entities;
using AutoAuthoring;
using System;

namespace Game.ECS.SimpleCollision
{
    public class DestroyThisHitAutoring : AutoAuthoring<DestroyThisHit> { }

    [Serializable]
    public struct DestroyThisHit : IComponentData { }
}