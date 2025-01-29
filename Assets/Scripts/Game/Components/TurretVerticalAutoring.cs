using UnityEngine;
using Unity.Entities;

namespace Game.ECS
{
    public class TurretVerticalAutoring : MonoBehaviour
    {
        [SerializeField]
        private float speed = 90;

        private class Baker : Baker<TurretVerticalAutoring>
        {
            public override void Bake(TurretVerticalAutoring authoring)
            {
                var entity = GetEntity(TransformUsageFlags.Dynamic);

                AddComponent(entity, new TurretVertical
                {
                    speed = authoring.speed
                });
            }
        }
    }

    public struct TurretVertical : IComponentData
    {
        public float speed;
    }
}