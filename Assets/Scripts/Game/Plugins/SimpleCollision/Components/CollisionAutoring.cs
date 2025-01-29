using UnityEngine;
using Unity.Entities;

namespace Game.ECS.SimpleCollision
{
    public struct Collision : IComponentData
    {
        public float radius;
    }

    public class CollisionAutoring : MonoBehaviour
    {
        [SerializeField]
        private float radius = 1;

        private class Baker : Baker<CollisionAutoring>
        {
            public override void Bake(CollisionAutoring authoring)
            {
                var entity = GetEntity(TransformUsageFlags.Dynamic);
                AddComponent(entity, new Collision
                {
                    radius = authoring.radius,
                });
                AddComponent(entity, new Hit());
                SetComponentEnabled<Hit>(entity, false);
            }
        }
    }
}
