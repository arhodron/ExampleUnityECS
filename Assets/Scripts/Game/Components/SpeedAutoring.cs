using Unity.Entities;
using System;

namespace Game.ECS
{
    public class SpeedAutoring : AutoAuthoring.AutoAuthoring<Speed> { }

    [Serializable]
    public struct Speed : IComponentData
    {
        public float value;
    }
}