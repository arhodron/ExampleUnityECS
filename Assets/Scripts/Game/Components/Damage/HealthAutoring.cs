using UnityEngine;
using Unity.Entities;
using Unity.Mathematics;
using System;
using AutoAuthoring;

namespace Game.ECS
{
    public class HealthAutoring : AutoAuthoring<Health> { }

    [Serializable]
    public struct Health : IComponentData
    {
        public float value;
    }
}