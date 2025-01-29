using Unity.Entities;
using AutoAuthoring;
using System;
using Unity.Mathematics;

namespace Game.ECS
{
    public class TargetValueAutoring : AutoAuthoring<TargetValue> { }

    [Serializable]
    public struct TargetValue : IComponentData, IEnableableComponent
    {
        public Entity value;
        public float3 position;
    }
}
