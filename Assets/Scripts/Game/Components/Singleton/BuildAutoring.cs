using UnityEngine;
using Unity.Entities;

namespace Game.ECS
{
    /// <summary>
    /// TODO:
    /// -interaction
    /// -icon
    /// </summary>
    public class BuildAutoring : MonoBehaviour
    {
        [SerializeField]
        private Build[] builds = new Build[0];

        private class Baker : Baker<BuildAutoring>
        {
            public override void Bake(BuildAutoring authoring)
            {
                foreach (var build in authoring.builds)
                    DependsOn(build.prefab);

                var entity = GetEntity(TransformUsageFlags.Dynamic);
                var buildings = AddBuffer<ECS.Build>(entity);

                foreach (var build in authoring.builds)
                {
                    buildings.Add(new ECS.Build
                    {
                        uidClick = Animator.StringToHash(build.nameClick),
                        entity = GetEntity(build.prefab, TransformUsageFlags.Dynamic),
                    });
                }
            }
        }

        [System.Serializable]
        public struct Build
        {
            public string nameClick;
            public GameObject prefab;
        }
    }

    [System.Serializable]
    public struct Build : IBufferElementData
    {
        public Entity entity;
        public int uidClick;

        public bool IsNull => uidClick == 0;
    }
}
