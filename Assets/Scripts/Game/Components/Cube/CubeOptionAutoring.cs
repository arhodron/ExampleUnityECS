using UnityEngine;
using Unity.Entities;
using Unity.Mathematics;
using System;

namespace Game.ECS
{
    public class CubeOptionAutoring : AutoAuthoring.AutoAuthoring<CubeOption> { }

    [Serializable]
    public struct CubeOption : IComponentData
    {
        public FlagCubeOption flags;
        [Obsolete("убрать и сделать для всех один пивот")]
        public float3 offset;
    }

    [Flags]
    public enum FlagCubeOption
    {
        Rotation = 1 << 0,
        IgnoreOtherAttach = 1 << 1,
        HasParent = 1 << 2,
        FullCollision = 1 << 3,
    }
}