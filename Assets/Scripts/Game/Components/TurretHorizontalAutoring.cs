using UnityEngine;
using Unity.Entities;

namespace Game.ECS
{
    public class TurretHorizontalAutoring : MonoBehaviour
    {
        [SerializeField]
        private float speed = 90;

        private class Baker : Baker<TurretHorizontalAutoring>
        {
            public override void Bake(TurretHorizontalAutoring authoring)
            {
                var entity = GetEntity(TransformUsageFlags.Dynamic);

                AddComponent(entity, new TurretHorizontal
                {
                    speed = authoring.speed
                });
            }
        }
    }

    public struct TurretHorizontal : IComponentData
    {
        public float speed;
    }
}
