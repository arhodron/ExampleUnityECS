using UnityEngine;
using Unity.Entities;
using Unity.Mathematics;
using System;

namespace Game.ECS
{
    public class DealDamageHitAutoring : AutoAuthoring.AutoAuthoring<DealDamageHit> { }

    [Serializable]
    public struct DealDamageHit : IComponentData { }
}