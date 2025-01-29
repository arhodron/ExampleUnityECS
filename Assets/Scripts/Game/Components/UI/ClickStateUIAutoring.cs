using UnityEngine;
using Unity.Entities;
using System;
using Game.Utility;

namespace Game.ECS.UI
{
    public class ClickStateUIAutoring : MonoBehaviour
    {
        [SerializeField]
        private string nameState;
        [SerializeField]
        private ClickState states;

        private class Baker : Baker<ClickStateUIAutoring>
        {
            public override void Bake(ClickStateUIAutoring authoring)
            {
                var entity = GetEntity(TransformUsageFlags.Dynamic);
                AddComponent(entity, new ClickStateUI
                {
                    uid = Animator.StringToHash(authoring.nameState),
                    states = authoring.states,
                });
            }
        }
    }

    [Serializable]
    public struct ClickStateUI : IComponentData, IEnableableComponent
    {
        public int uid;
        public ClickState states;

        [NonSerialized]
        private ClickState current;

        public void UpdateState(bool enabled)
        {
            switch (current)
            {
                case ClickState.None:
                    current = enabled ? ClickState.Down : ClickState.None;
                    return;
                case ClickState.Down:
                    current = enabled ? ClickState.Press : ClickState.None;
                    return;
                case ClickState.Press:
                    current = enabled ? ClickState.Press : ClickState.Up;
                    return;
                case ClickState.Up:
                    current = enabled ? ClickState.Down : ClickState.None;
                    return;
            }
            current = ClickState.None;
        }

        public void Reset()
        {
            current = ClickState.None;
        }

        public bool HasState() =>
            EnumUtility.HasFlagUnsafe(states, current);
    }

    [Flags]
    public enum ClickState
    {
        None = 1 << 0,
        Down = 1 << 1,
        Press = 1 << 2,
        Up = 1 << 3,
    }
}