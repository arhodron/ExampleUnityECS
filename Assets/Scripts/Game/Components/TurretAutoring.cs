using UnityEngine;
using Unity.Entities;

namespace Game.ECS
{
    public class TurretAutoring : MonoBehaviour
    {
        [SerializeField]
        private Transform horizontal = null;
        [SerializeField]
        private Transform vertical = null;
        [SerializeField]
        private float speed = 10;

        private class Baker : Baker<TurretAutoring>
        {
            public override void Bake(TurretAutoring authoring)
            {
                var entity = GetEntity(TransformUsageFlags.Dynamic);

                SetTurretRotation();

                AddComponent(entity, new CreateEvent());

                void SetTurretRotation()
                {
                    DependsOn(authoring.horizontal);
                    DependsOn(authoring.vertical);

                    if (authoring.horizontal == null || authoring.vertical == null)
                        return;

                    if (authoring.horizontal != null && authoring.vertical != null)
                    {
                        AddComponent(entity, new TurretRotation
                        {
                            horizontal = GetEntity(authoring.horizontal, TransformUsageFlags.Dynamic),
                            vertical = GetEntity(authoring.vertical, TransformUsageFlags.Dynamic),
                            speed = authoring.speed
                        });
                    }
                }
            }
        }
    }

    public struct Turret : IComponentData { }

    public struct TurretRotation : IComponentData
    {
        public Entity horizontal;
        public Entity vertical;
        public float speed;
    }
}