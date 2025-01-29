using UnityEngine;
using Unity.Entities;
using Unity.Mathematics;
using System;

namespace Game.ECS
{
    public class TakeDamageAutoring : AutoAuthoring.BufferAutoAuthoring<TakeDamage> { }

    [Serializable]
    public struct TakeDamage : IBufferElementData, IEnableableComponent
    {
        public float value;
    }
}