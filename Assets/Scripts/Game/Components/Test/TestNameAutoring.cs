using UnityEngine;
using Unity.Entities;
using Unity.Collections;

namespace Game.ECS.Test
{
    public class TestNameAutoring : MonoBehaviour
    {
        private class Baker : Baker<TestNameAutoring>
        {
            public override void Bake(TestNameAutoring authoring)
            {
                var entity = GetEntity(TransformUsageFlags.Dynamic);
                AddComponent(entity, new TestName
                {
                    name = authoring.gameObject.name
                });
            }
        }
    }

    public struct TestName : IComponentData
    {
        public FixedString64Bytes name;
    }
}