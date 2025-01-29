using Game.ECS;
using Unity.Burst;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

namespace Game.Utility
{
    [BurstCompile]
    public static partial class TurretUtility
    {
        [BurstCompile]
        public static void RotationHorizontal(in float3 target, in float3 position, in float3 up, out quaternion rotation)
        {
            float3 vecToTarget = target - position;
            MathUtility.ProjectOnPlane(vecToTarget, up, out float3 forvard);

            rotation = quaternion.LookRotation(math.normalize(forvard), up);
        }

        [BurstCompile]
        public static void RotationVertical(in float3 target, in float3 position, in float4x4 parent, ref quaternion rotation)
        {
            float3 localTarget = TransformHelpers.InverseTransformDirection(parent, target - position);

            MathUtility.ProjectOnPlane(localTarget, new float3(0, 1, 0), out float3 flattened);

            MathUtility.Angle(flattened, localTarget, out float elevation);
            elevation *= math.sign(localTarget.y);

            elevation = math.clamp(elevation * math.TODEGREES, -10, 90) / math.TODEGREES;

            if (math.abs(elevation * math.TODEGREES) > math.EPSILON)
                rotation = quaternion.Euler(new float3(1, 0, 0) * -elevation);
        }

        [BurstCompile]
        public static void PredictiveProjectile(in float3 turretPosition, in float projectileSpeed, in float3 targetPosition, in float3 targetVelocity, out float3 res)
        {
            float3 direction = targetPosition - turretPosition;
            MathUtility.Angle(-direction, targetVelocity, out float targetMoveAngle);// * math.TODEGREES;
            //float targetMoveAngle = Vector3.Angle(-displacement, targetVelocity) * math.Deg2Rad;

            //if the target is stopping or if it is impossible for the projectile to catch up with the target (Sine Formula)
            float targetVelocityMagnitude = math.length(targetVelocity);
            if (targetVelocityMagnitude == 0 || targetVelocityMagnitude > projectileSpeed && math.sin(targetMoveAngle) / projectileSpeed > math.cos(targetMoveAngle) / targetVelocityMagnitude)
            {
                res = targetPosition;
            }
            else
            {
                float shootAngle = math.asin(math.sin(targetMoveAngle) * targetVelocityMagnitude / projectileSpeed);
                res = targetPosition + targetVelocity * math.length(direction) / math.sin(math.PI - targetMoveAngle - shootAngle) * math.sin(shootAngle) / targetVelocityMagnitude;
            }
        }
    }
}

