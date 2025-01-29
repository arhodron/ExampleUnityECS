using Unity.Entities;
using System;

namespace Game.ECS
{
    public class LifeTimeAutoring : AutoAuthoring.AutoAuthoring<LifeTime> { }

    [Serializable]
    public struct LifeTime : IComponentData
    {
        public float value;
    }
}