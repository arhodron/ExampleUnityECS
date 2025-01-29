using UnityEngine;
using Unity.Entities;

namespace Game.ECS
{
    public class AsteroidSpawnerAutoring : MonoBehaviour
    {
        [SerializeField]
        private GameObject prefab;
        [SerializeField]
        private float rangeSpawn = 20;
        [SerializeField]
        private float rangeTarget = 5;
        [SerializeField]
        private float spawnRate = 0.5f;

        private class Baker : Baker<AsteroidSpawnerAutoring>
        {
            public override void Bake(AsteroidSpawnerAutoring authoring)
            {
                DependsOn(authoring.prefab);
                if (authoring.prefab == null)
                    return;

                var entity = GetEntity(TransformUsageFlags.Dynamic);
                var prefab = GetEntity(authoring.prefab, TransformUsageFlags.Dynamic);

                AddComponent(entity, new AsteroidSpawner
                {
                    prefab = prefab,
                    rangeSpawn = authoring.rangeSpawn,
                    rangeTarget = authoring.rangeTarget,
                    spawnRate = authoring.spawnRate,
                });
            }
        }
    }

    public struct AsteroidSpawner : IComponentData
    {
        public Entity prefab;
        public float rangeSpawn;
        public float rangeTarget;
        public float spawnRate;
        public double timeRate;
    }
}