using UnityEngine;
using Unity.Entities;

namespace Game.ECS
{
    public class TurretFireAutoring : MonoBehaviour
    {
        [SerializeField]
        private GameObject projectile = null;
        [SerializeField]
        private Transform pivot = null;
        [SerializeField]
        private float fireRate = 1;

        private class Baker : Baker<TurretFireAutoring>
        {
            public override void Bake(TurretFireAutoring authoring)
            {
                DependsOn(authoring.projectile);
                DependsOn(authoring.pivot);

                if (authoring.projectile == null || authoring.pivot == null)
                    return;

                var entity = GetEntity(TransformUsageFlags.Dynamic);

                AddComponent(entity, new TurretFire
                {
                    projectile = GetEntity(authoring.projectile, TransformUsageFlags.Dynamic),
                    pivot = GetEntity(authoring.pivot, TransformUsageFlags.Dynamic),
                    fireRate = authoring.fireRate,
                });
            }
        }
    }

    public struct TurretFire : IComponentData
    {
        public Entity projectile;
        public Entity pivot;
        public float fireRate;
        public double fireTime;
    }
}
