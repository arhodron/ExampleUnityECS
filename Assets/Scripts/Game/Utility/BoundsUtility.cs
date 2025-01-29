using Unity.Burst;
using Unity.Mathematics;

namespace Game.Utility
{
    [BurstCompile]
    public static class BoundsUtility
    {
        [BurstCompile]
        public static void GetMin(in float3 center, in float3 size, out float3 res)
        {
            res = new float3(center - size * 0.5f);
        }

        [BurstCompile]
        public static void GetMax(in float3 center, in float3 size, out float3 res)
        {
            res = new float3(center + size * 0.5f);
        }

        [BurstCompile]
        public static void IsContaints(in float3 center, in float3 size, in float3 target, out bool res)
        {
            GetMin(center, size, out float3 min);
            GetMax(center, size, out float3 max);
            res = target.x >= min.x && target.x <= max.x &&
                target.y >= min.y && target.y <= max.y &&
                target.z >= min.z && target.z <= max.z;
        }

        [BurstCompile]
        public static void Intersection(in float3 center, in float3 size, in float3 origin, in float3 direction, out bool res) 
        {
            float tmin = 0f;
            float tmax = float.MaxValue;
            float3 inv = 1 / direction;

            GetMin(center, size, out float3 min);
            GetMax(center, size, out float3 max);

            for (int d = 0; d < 3; ++d)
            {
                float t1 = (min[d] - origin[d]) * inv[d];
                float t2 = (max[d] - origin[d]) * inv[d];

                tmin = math.max(tmin, math.min(t1, t2));
                tmax = math.min(tmax, math.max(t1, t2));
            }

            res = tmin < tmax;
        }


        [BurstCompile]
        public static void Intersection2(in float3 center, in float3 size, in float3 origin, in float3 direction, out float res)
        {
            float tmin = 0f;
            float tmax = float.MaxValue;
            float3 inv = 1 / direction;

            GetMin(center, size, out float3 min);
            GetMax(center, size, out float3 max);

            for (int d = 0; d < 3; ++d)
            {
                float t1 = (min[d] - origin[d]) * inv[d];
                float t2 = (max[d] - origin[d]) * inv[d];

                tmin = math.max(tmin, math.min(t1, t2));
                tmax = math.min(tmax, math.max(t1, t2));
            }
             
            res = tmin < tmax ? tmin : 0;
        }

        [BurstCompile]
        public static void Raycast(in float3 center, in float3 size, in float3 origin, in float3 direction, out float res)
        {
            float tmin = 0f;
            float tmax = float.MaxValue;
            float3 inv = 1 / direction;

            GetMin(center, size, out float3 min);
            GetMax(center, size, out float3 max);

            for (int d = 0; d < 3; ++d)
            {
                float t1 = (min[d] - origin[d]) * inv[d];
                float t2 = (max[d] - origin[d]) * inv[d];

                tmin = math.max(tmin, math.min(t1, t2));
                tmax = math.min(tmax, math.max(t1, t2));
            }

            if (tmin < tmax)
            {
                res = tmin;
                if (math.distance(origin, origin + direction * tmin) > math.length(direction))
                    res = 0;
                else
                    res = math.length(direction * res);
            }
            else
            {
                res = 0;
            }
        }

        public static void Intersection3(in float3 center, in float3 size, in float3 origin, in float3 direction, out float res)
        {
            GetMin(center, size, out float3 min);
            GetMax(center, size, out float3 max);

            float[] t = new float[10];
            t[1] = (min.x - origin.x) / direction.x;
            t[2] = (max.x - origin.x) / direction.x;
            t[3] = (min.y - origin.y) / direction.y;
            t[4] = (max.y - origin.y) / direction.y;
            t[5] = (min.z - origin.z) / direction.z;
            t[6] = (max.z - origin.z) / direction.z;
            t[7] = math.max(math.max(math.min(t[1], t[2]), math.min(t[3], t[4])), math.min(t[5], t[6]));
            t[8] = math.min(math.min(math.max(t[1], t[2]), math.max(t[3], t[4])), math.max(t[5], t[6]));
            t[9] = (t[8] < 0 || t[7] > t[8]) ? 0 : t[7];
            res = t[9];
        }

        /*public static void boxIntersection(in float3 ro, in float3 rd, float3 boxSize, out vec3 outNormal)
        {
            vec3 m = 1.0 / rd; // can precompute if traversing a set of aligned boxes
            vec3 n = m * ro;   // can precompute if traversing a set of aligned boxes
            vec3 k = abs(m) * boxSize;
            vec3 t1 = -n - k;
            vec3 t2 = -n + k;
            float tN = max(max(t1.x, t1.y), t1.z);
            float tF = min(min(t2.x, t2.y), t2.z);
            if (tN > tF || tF < 0.0) return vec2(-1.0); // no intersection
            outNormal = (tN > 0.0) ? step(vec3(tN), t1)) : // ro ouside the box
                           step(t2, vec3(tF)));  // ro inside the box

            return vec2(tN, tF);
        }*/
    }
}
