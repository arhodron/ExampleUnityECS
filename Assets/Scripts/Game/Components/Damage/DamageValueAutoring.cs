using UnityEngine;
using Unity.Entities;
using Unity.Mathematics;
using System;

namespace Game.ECS
{
    public class DamageValueAutoring : AutoAuthoring.AutoAuthoring<DamageValue> { }

    [Serializable]
    public struct DamageValue : IComponentData
    {
        public float value;
    }
}