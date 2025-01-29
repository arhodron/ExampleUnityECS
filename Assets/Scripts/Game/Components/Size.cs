using Unity.Entities;
using Unity.Mathematics;

namespace Game
{
    public struct Size : IComponentData
    {
        public int3 size;
    }
}
