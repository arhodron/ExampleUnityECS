using Unity.Entities;
using AutoAuthoring;
using System;

namespace Game.ECS
{
    public class TargetRangeAutoring : AutoAuthoring<TargetRange> { }

    [Serializable]
    public struct TargetRange : IComponentData
    {
        public float value;
    }
}
