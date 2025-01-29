using UnityEngine;
using System.Collections.Generic;
using System;
using UnityEngine.UI;
using UnityEngine.EventSystems;

namespace Game.UI
{
    public abstract class StateUIMono<V> : MonoBehaviour
        where V : IEquatable<V>
    {
        [SerializeField]
        private string nameState = "";
        [NonSerialized]
        private int uid;

        private StateUI<V> state = null;

        public V State 
        { 
            get => state.Value;
            set
            {
                if (IsLockChange)
                    return;

                IsLockChange = true;
                state.Value = value;
                IsLockChange = false;
            }
        }
        protected bool IsLockChange { private set; get; }

        private void Awake()
        {
            uid = Animator.StringToHash(nameState);

            AwakeState();
        }

        protected virtual void AwakeState() { }

        private void OnEnable()
        {
            state = StatesUIMono<V>.Instance.CreateState(uid);

            state.onChangeValue -= OnChangeValue;
            state.onChangeValue += OnChangeValue;

            OnEnableState();
        }

        protected virtual void OnEnableState() { }

        private void OnDisable()
        {
            RemoveState();
        }

        private void OnDestroy()
        {
            RemoveState();
        }

        private void RemoveState()
        {
            if (state == null)
                return;

            if(state.instances == 1)
                Reset();

            state.onChangeValue -= OnChangeValue;

            StatesUIMono<V>.Instance.RemoveState(uid);
            state = null;
        }

        protected void OnChangeValue(V current, V last)
        {
            if (IsLockChange == false)
                OnChangeState(current, last);
        }

        protected virtual void OnChangeState(V current, V last) { }

        protected virtual void Reset() => state.Value = default;
    }
}
