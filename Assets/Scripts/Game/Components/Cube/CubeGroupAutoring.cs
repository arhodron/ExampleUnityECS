using UnityEngine;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Burst;
using System;
using Game.Utility;
using Plugins.Collections;

namespace Game.ECS
{
    public class CubeGroupAutoring : MonoBehaviour
    {
        [SerializeField]
        private Vector3Int size = new Vector3Int(10, 10, 10);

        private class Baker : Baker<CubeGroupAutoring>
        {
            public override void Bake(CubeGroupAutoring authoring)
            {
                var entity = GetEntity(TransformUsageFlags.Dynamic);
                AddComponent(entity, new CubeGroup
                {
                    size = new int3(authoring.size.x, authoring.size.y, authoring.size.z),
                });
                AddComponent(entity, new CreateEvent());
            }
        }
    }

    [BurstCompile]
    [ChunkSerializable]
    public struct CubeGroup : IComponentData, IDisposable
    {
        private const int PART_COLLISION = 1;
        private const int FULL_COLLISION = 2;

        private const int PART_COLLISION_MASK = 1 << PART_COLLISION;
        private const int ALL_COLLISION_MASK = 1 << PART_COLLISION | 1 << FULL_COLLISION;


        public int3 size;

        public int Lenght => size.x * size.y * size.z;

        public bool IsEmpty => entities.IsCreated == false && collisions.IsCreated == false;

        [NoAlias]
        private UnsafeArray<Entity> entities;
        [NoAlias]
        private UnsafeArray<int> collisions;

        public void Init()
        {
            if (!IsEmpty)
                return;

            int lenght = Lenght;

            entities = new UnsafeArray<Entity>(lenght, Allocator.Persistent);

            collisions = new UnsafeArray<int>(lenght, Allocator.Persistent);
        }

        [BurstCompile]
        public void Raycast(in float3 position, in quaternion rotation, in float3 origin, in float3 direction, out int3 hit, out int3 normal, out bool res, bool isFullCollision = true)
        {
            CubeUtility.Raycast(collisions, isFullCollision ? ALL_COLLISION_MASK : PART_COLLISION_MASK, size, position, rotation,
                origin, direction, out hit, out normal, out res);
        }

        [BurstCompile]
        public void GetIndex(int3 position, out int res)
        {
            IsRange(position, out bool isRange);
            if (isRange)
                CubeUtility.ToIndex(position, size, out res);
            else
                res = -1;
        }

        [BurstCompile]
        public void SetEntity(int index, Entity entity, bool isFullCollision)
        {
            entities[index] = entity;
            collisions[index] = isFullCollision ? FULL_COLLISION : PART_COLLISION;
        }

        [BurstCompile]
        public void RemoveEntity(int index)
        {
            entities[index] = default;
            collisions[index] = 0;
        }

        [BurstCompile]
        public void GetEntity(int index, out Entity res)
        {
            res = entities[index];
        }

        [BurstCompile]
        public void GetEntity(int3 position, out Entity res)
        {
            GetIndex(position, out int index);
            if (index >= 0)
                GetEntity(index, out res);
            else
                res = default;
        }

        [BurstCompile]
        public void GetNotFullCollision(ref int3 position, in int3 normal)
        {
            IsRange(position + normal, out bool isRange);
            if (!isRange)
                return;

            HasCollision(position + normal, out bool isCollision);
            if (isCollision)
            {
                position += normal;
                return;
            }
        }

        [BurstCompile]
        public void HasCollision(in int index, out bool res)
        {
            res = collisions[index] != 0;
        }

        [BurstCompile]
        public void HasCollision(in int3 position, out bool res)
        {
            GetIndex(position, out int index);
            if (index >= 0)
                HasCollision(index, out res);
            else
                res = false;
        }

        [BurstCompile]
        public void HasEntity(int index, out bool res)
        {
            res = entities[index] != Entity.Null;
        }

        [BurstCompile]
        public void HasEntity(int3 position, out bool res)
        {
            GetIndex(position, out int index);
            if (index >= 0)
                HasEntity(index, out res);
            else
                res = false;
        }

        [BurstCompile]
        public void IsRange(in int3 position, out bool res)
        {
            CubeUtility.IsRange(position, size, out res);
        }

        [BurstCompile]
        public void IsRange(in int index, out bool res)
        {
            res = index >= 0 && index < entities.Length;
        }

        [BurstCompile]
        public void HasCube(in int3 position, out bool res)
        {
            CubeUtility.ToIndex(position, size, out int index);
            HasCube(index, out res);
        }

        [BurstCompile]
        public void HasCube(in int index, out bool res)
        {
            res = collisions[index] != 0;
        }

        #region IDisposable

        public void Dispose()
        {
            entities.Dispose();
            collisions.Dispose();
        }

        #endregion
    }
}