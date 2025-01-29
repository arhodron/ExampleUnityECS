using Unity.Entities.Serialization;
using Unity.Collections;
using UnityEngine;
using Unity.Entities;

namespace Game.ECS
{
    public class PlayerControllerAutoring : MonoBehaviour
    {
        [SerializeField]
        private GameObject select;

        private class Baker : Baker<PlayerControllerAutoring>
        {
            public override void Bake(PlayerControllerAutoring authoring)
            {
                DependsOn(authoring.select);

                if (authoring.select == null)
                    return;

                var entity = GetEntity(TransformUsageFlags.None);

                var select = GetEntity(authoring.select, TransformUsageFlags.Dynamic);

                AddComponent(entity, new PlayerController
                {
                    select = select,
                });
            }
        }
    }

    public struct PlayerController : IComponentData
    {
        public Entity select;
    }
}