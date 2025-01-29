using Unity.Burst;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Mathematics;

namespace Game.Utility
{
    [BurstCompile]
    public static partial class CubeUtility
    {
        [BurstCompile]
        public static void GetLenght(in int3 size, out int res)
        {
            res = size.x * size.y * size.z;
        }

        [BurstCompile]
        public static void IsRange(in int3 value, in int3 size, out bool res)
        {
            res = value.x >= 0 && value.y >= 0 && value.z >= 0 &&
                value.x < size.x && value.y < size.y && value.z < size.z;
        }

        [BurstCompile]
        public static void IsRange(in int3 value, in int3 size, int offset, out bool res)
        {
            res = value.x >= offset && value.y >= offset && value.z >= offset &&
                value.x < size.x - offset && value.y < size.y - offset && value.z < size.z - offset;
        }

        [BurstCompile]
        public static void ToIndex(in int3 position, in int3 size, out int res)
        {
            res = position.x + position.z * size.x + position.y * size.x * size.y;
        }

        [BurstCompile]
        public static void ToPosition(in int index, in int3 size, out int3 res)
        {
            int y = index / (size.x * size.z);
            int indexxz = index - y * size.x * size.z;
            int x = indexxz % size.z;
            int z = indexxz / size.z;

            res = new int3(x, y, z);
        }

        [BurstCompile]
        public static void Raycast(in UnsafeList<byte> cubes, in byte collision, in int3 size, in float3 position, in float3 origin, in float3 direction, out int3 hit, out int3 normal, out bool res)
        {
            float3 orig = origin;
            orig -= position;
            Raycast(cubes, collision, size, orig, direction, out hit, out normal, out res);
        }

        [BurstCompile]
        public static void Raycast(in UnsafeList<byte> cubes, in byte collision, in int3 size, in float3 position, in quaternion rotation, in float3 origin, in float3 direction, out int3 hit, out int3 normal, out bool res)
        {
            float3 orig = origin;
            float3 dir = direction;

            ToLocal(ref orig, ref dir, position, rotation);

            Raycast(cubes, collision, size, orig, dir, out hit, out normal, out res);
        }

        [BurstCompile]
        public static void ToLocal(ref float3 origin, ref float3 direction, in float3 position, in quaternion rotation)
        {
            origin -= position;
            quaternion rot = math.inverse(rotation);
            origin = math.rotate(rot, origin);
            direction = math.rotate(rot, direction);
        }

        [BurstCompile]
        public static void SingleNormalize(ref float3 normal)
        {
            if (normal.x > normal.y && normal.x > normal.z)
                normal.y = normal.z = 0;
            else if (normal.y > normal.x && normal.y > normal.z)
                normal.x = normal.z = 0;
            else if (normal.z > normal.x && normal.z > normal.y)
                normal.x = normal.y = 0;

            normal = math.normalize(normal);
        }

        [BurstCompile]
        public static void Raycast(in UnsafeList<byte> cubes, in byte collision, in int3 size, in float3 origin, in float3 direction, out int3 hit, out int3 normal, out bool res)
        {
            const float offset_value = 0.01f;

            float3 dir = direction;
            float3 orig = origin;

            float3 normalize = math.normalize(dir);

            ToInt(orig, out hit);
            IsRange(hit, size, out bool isRange);
            if (isRange == false)
            {
                float d = math.length(dir);
                BoundsUtility.Raycast((float3)size * 0.5f, size, orig, dir, out float distBounds);;
                if (distBounds != 0)
                {
                    d -= distBounds - offset_value;
                    dir = normalize * d;
                    orig = orig + normalize * (distBounds - offset_value);
                }
                else
                {
                    hit = int3.zero;
                    normal = int3.zero;
                    res = false;
                    return;
                }
            }
            else
            {
                HasCube(cubes, collision, size, hit, out bool isHas);
                if (isHas)
                {
                    ToInt(orig - normalize * offset_value, out normal);
                    normal -= hit;
                    res = true;
                    return;
                }
            }

            float distance = math.length(dir);

            float3 lenght = new float3(
                math.length(math.project(dir, new float3(1, 0, 0))),
                math.length(math.project(dir, new float3(0, 1, 0))),
                math.length(math.project(dir, new float3(0, 0, 1)))
                );

            float3 kof = new float3(
                math.abs(distance / lenght.x),
                math.abs(distance / lenght.y),
                math.abs(distance / lenght.z));

            float3 offset = 0;
            ToOffset(orig.x, dir.x, out offset.x);
            ToOffset(orig.y, dir.y, out offset.y);
            ToOffset(orig.z, dir.z, out offset.z);

            float3 offset_delta = normalize * offset_value;

            float dist = float.MaxValue;

            for (int i = 0; i < 3; i++)
            {
                int count = -1;
                while (++count < lenght[i])
                {
                    float d = kof[i] * (count - offset[i]);
                    if (d > distance)
                        break;

                    if (d < 0)
                        continue;

                    float3 p = orig + normalize * (d + offset_value);
                    ToInt(p, out int3 h);

                    IsRange(h, size, out isRange);
                    if (isRange == false)
                        break;

                    HasCube(cubes, collision, size, h, out bool isHas);
                    if (isHas)
                    {
                        if (dist > d)
                        {
                            hit = h;
                            dist = d;
                        }
                        break;
                    }
                }
            }

            if (dist == float.MaxValue)
            {
                normal = int3.zero;
                res = false;
                return;
            }
            else
            {
                float3 raw = orig + normalize * dist;
                GetNormal(raw - offset_delta, hit, out normal);
                res = true;
                return;
            }
        }

        [BurstCompile]
        private static void HasCube(in UnsafeList<byte> cubes, in int collision, in int3 size, in int3 position, out bool res)
        {
            ToIndex(position, size, out int index);
            res = cubes[index] >= collision;
        }

        [BurstCompile]
        private static void ToOffset(in float value, in float sign, out float res)
        {
            if (sign >= 0)
                res = value > 0 ? value - math.floor(value) : value - math.ceil(value);
            else
                res = -(value > 0 ? value - math.floor(value) : value - math.ceil(value));
        }

        [BurstCompile]
        private static void GetNormal(in float3 start, in int3 end, out int3 res)
        {
            int index = 0;
            float dist = float.MaxValue;
            for (int i = 0; i < 6; i++)
            {
                float3 tr = end + DIR[i];
                tr += 0.5f;
                float d = math.distance(start, tr);
                if (d < dist)
                {
                    dist = d;
                    index = i;
                }
            }
            res = DIR[index];
        }

        [BurstCompile]
        public static void ToInt(in float3 value, out int3 res)
        {
            res = new int3((int)math.floor(value.x), (int)math.floor(value.y), (int)math.floor(value.z));
        }

        public static readonly int3[] DIR = new int3[]
        {
            new int3(1, 0, 0),
            new int3(-1, 0, 0),
            new int3(0, 1, 0),
            new int3(0, -1, 0),
            new int3(0, 0, 1),
            new int3(0, 0, -1),
        };
    }
}