using Unity.Entities;
using Unity.Mathematics;

namespace Game.ECS
{
    public struct GameCamera : IComponentData
    {
        public float2 angles;
        public float3 position;
        public quaternion rotation;
        public float3 origin;
        public float3 direction;
    }
}
