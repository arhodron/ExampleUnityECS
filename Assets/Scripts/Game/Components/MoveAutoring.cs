using Unity.Entities;
using Unity.Mathematics;
using System;

namespace Game.ECS
{
    public class MoveAutoring : AutoAuthoring.AutoAuthoring<Move> { }

    [Serializable]
    public struct Move : IComponentData
    {
        public float3 direction;
    }
}
