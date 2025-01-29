using Unity.Entities;
using AutoAuthoring;
using System;

namespace Game.ECS
{
    public class TargetAutoring : AutoAuthoring<Target> { }

    [Serializable]
    public struct Target : IComponentData { }
}