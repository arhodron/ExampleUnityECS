using UnityEngine;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Burst;
using System;

namespace Game.ECS
{
    public class TargetFinderAutoring : MonoBehaviour
    {
        [SerializeField]
        private TargetFinder finder;

        private class Baker : Baker<TargetFinderAutoring>
        {
            public override void Bake(TargetFinderAutoring authoring)
            {
                var entity = GetEntity(TransformUsageFlags.Dynamic);
                AddComponent(entity, authoring.finder);
            }
        }
    }

    [Serializable]
    [ChunkSerializable]
    public struct TargetFinder : IComponentData, IEnableableComponent, IDisposable
    {
        [NoAlias]
        public UnsafeList<TargetPosition> cache;
        public float timeRate;
        [HideInInspector]
        private float time;
        [HideInInspector]
        public bool state;

        public bool UpdateTimeRate(float deltaTime)
        {
            time -= deltaTime;
            if (time <= 0)
            {
                time = timeRate;
                return true;
            }
            else
                return false;
        }

        public void ResetTimeRate() => time = timeRate;

        #region IDisposable

        public void Dispose()
        {
            cache.Dispose();
        }

        #endregion
    }

    [Obsolete("может быть использован как альтернатива unsafe list")]
    public struct TargetList : IBufferElementData
    {
        public Entity value;
    }

    public struct TargetPosition
    {
        public Entity value;
        public float3 position;

        public TargetPosition(Entity value, float3 position)
        {
            this.value = value;
            this.position = position;
        }
    }
}
