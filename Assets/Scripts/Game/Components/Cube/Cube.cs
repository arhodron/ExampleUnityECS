using UnityEngine;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Entities.Serialization;
using Unity.Collections;

namespace Game.ECS
{
    public struct Cube : IComponentData
    {
        public int3 position;
        public int3 normal;
        public int index;
    }
}
