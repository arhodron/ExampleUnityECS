using UnityEngine;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Burst;
using System;
using Game.Utility;

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
        private const byte PART_COLLISION = 1;
        private const byte FULL_COLLISION = 2;

        public int3 size;

        public int Lenght => size.x * size.y * size.z;

        public bool IsEmpty => entities.IsCreated == false && collisions.IsCreated == false;

        [NoAlias]
        private UnsafeList<Entity> entities;
        [NoAlias]
        private UnsafeList<byte> collisions;

        ///TODO: replace collisions unsafe list to custom unsafe array
        public void Init()
        {
            if (!IsEmpty)
                return;

            int lenght = Lenght;

            entities = new UnsafeList<Entity>(lenght, Allocator.Persistent);
            for (int i = 0; i < lenght; i++)
                entities.Add(default);

            collisions = new UnsafeList<byte>(lenght, Allocator.Persistent);
            for (int i = 0; i < lenght; i++)
                collisions.Add(default);
        }
        public void Raycast(in float3 position, in quaternion rotation, in float3 origin, in float3 direction, out int3 hit, out int3 normal, out bool res, bool isFullCollision = true)
        {
            CubeUtility.Raycast(collisions, isFullCollision ? FULL_COLLISION : PART_COLLISION, size, position, rotation,
                origin, direction, out hit, out normal, out res);
        }

        public void GetIndex(int3 position, out int res)
        {
            IsRange(position, out bool isRange);
            if (isRange)
                CubeUtility.ToIndex(position, size, out res);
            else
                res = -1;
        }

        public void SetEntity(int index, Entity entity, bool isFullCollision)
        {
            entities[index] = entity;
            collisions[index] = isFullCollision ? FULL_COLLISION : PART_COLLISION;
        }

        public void RemoveEntity(int index)
        {
            entities[index] = default;
            collisions[index] = 0;
        }

        public void GetEntity(int index, out Entity res)
        {
            res = entities[index];
        }

        public void GetEntity(int3 position, out Entity res)
        {
            GetIndex(position, out int index);
            if (index >= 0)
                GetEntity(index, out res);
            else
                res = default;
        }

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

        public void HasCollision(in int index, out bool res, int collision = PART_COLLISION)
        {
            res = collisions[index] >= collision;
        }

        public void HasCollision(int3 position, out bool res, int collision = PART_COLLISION)
        {
            GetIndex(position, out int index);
            if (index >= 0)
                HasCollision(index, out res, collision);
            else
                res = false;
        }

        public void HasEntity(int index, out bool res)
        {
            res = entities[index] != Entity.Null;
        }

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