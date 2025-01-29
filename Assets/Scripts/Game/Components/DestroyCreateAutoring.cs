using UnityEngine;
using Unity.Entities;

namespace Game.ECS
{
    public class DestroyCreateAutoring : MonoBehaviour
    {
        [SerializeField]
        private GameObject prefab;

        private class Baker : Baker<DestroyCreateAutoring>
        {
            public override void Bake(DestroyCreateAutoring authoring)
            {
                DependsOn(authoring.prefab);
                if (authoring.prefab == null)
                    return;

                var entity = GetEntity(TransformUsageFlags.Dynamic);
                AddComponent(entity, new DestroyCreate
                {
                    prefab = GetEntity(authoring.prefab, TransformUsageFlags.Dynamic),
                });
            }
        }
    }

    [System.Serializable]
    public struct DestroyCreate : IComponentData
    {
        public Entity prefab;
    }
}