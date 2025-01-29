using Unity.Burst;
using Unity.Mathematics;

namespace Game.Utility
{
    [BurstCompile]
    public static partial class MathUtility
    {
        [BurstCompile]
        public static void ProjectOnPlane(in float3 vector, in float3 planeNormal, out float3 res)
        {
            float sqrMag = math.dot(planeNormal, planeNormal);
            if (sqrMag < math.EPSILON)
            {
                res = vector;
            }
            else
            {
                var dot = math.dot(vector, planeNormal);
                res = vector - planeNormal * dot / sqrMag;
            }
        }

        [BurstCompile]
        public static void Angle(in float3 from, in float3 to, out float res) 
        { 
            res = math.acos(math.clamp(math.dot( math.normalize(from), math.normalize(to)), -1F, 1F));
        }

        [BurstCompile]
        public static void RotateTowards(in quaternion from, in quaternion to, in float maxDegreesDelta, out quaternion res)
        {
            Angle(from, to, out float angle);
            res = angle < float.Epsilon ? to : math.slerp(from, to, math.min(1f, maxDegreesDelta / angle));
        }

        [BurstCompile]
        public static void Angle(in quaternion q1, in quaternion q2, out float res)
        {
            var dot = math.dot(q1, q2);
            res = !(dot > 0.999998986721039) ? (float)(math.acos(math.min(math.abs(dot), 1f)) * 2.0) : 0.0f;
        }
    }
}
