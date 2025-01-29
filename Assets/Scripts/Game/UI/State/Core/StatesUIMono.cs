using UnityEngine;
using System.Collections.Generic;
using System;
using UnityEngine.UI;
using UnityEngine.EventSystems;

namespace Game.UI
{
    public abstract class StatesUIMono<V> : MonoBehaviour
        where V : IEquatable<V>
    {
        public static StatesUIMono<V> Instance { get; private set; } = null;

        [Obsolete("replace to data array")]
        private Dictionary<int, StateUI<V>> states = new Dictionary<int, StateUI<V>>();

        private void Awake()
        {
            Instance = this;
        }

        internal StateUI<V> CreateState(int uid)
        {
            if (GetState(uid, out var state))
            {
                state.instances++;
                return state;
            }
            else
            {
                state = new StateUI<V>(default);
                state.instances++;
                states.Add(uid, state);
                return state;
            }
        }

        public bool RemoveState(int uid)
        {
            if (GetState(uid, out var state))
            {
                state.instances--;
                if (state.instances <= 0)
                {
                    state.Dispose();
                    states.Remove(uid);
                    return true;
                }
            }

            return false;
        }

        private bool GetState(int uid, out StateUI<V> res)
        {
            if (states.ContainsKey(uid))
            {
                res = states[uid];
                return true;
            }
            else
            {
                res = default;
                return false;
            }
        }

        public V GetStateValue(int uid)
        {
            if (GetState(uid, out var state))
                return state.Value;
            else
                return default;
        }

        public bool GetStateValue(int uid, out V res)
        {
            if (GetState(uid, out var state))
            {
                res = state.Value;
                return true;
            }
            else
            {
                res = default;
                return false;
            }
        }

        public void SetStateValue(int uid, V value)
        {
            if (GetState(uid, out var state))
                state.Value = value;
        }

        private void OnDestroy()
        {
            Instance = null;
        }
    }

    public class StateUI<V> : IDisposable
        where V : IEquatable<V>
    {
        private V value = default;
        public V Value
        {
            get => value;
            set
            {
                if (this.value.Equals(value) == false)
                {
                    lastValue = this.value;
                    this.value = value;

                    onChangeValue?.Invoke(value, lastValue);
                }
            }
        }

        private V lastValue = default;
        public V LastValue => lastValue;

        internal int instances = 0;

        public StateUI(V value)
        {
            this.value = value;
            lastValue = value;
        }

        public event Action<V, V> onChangeValue;

        #region IDisposable

        public void Dispose()
        {
            onChangeValue = null;
        }

        #endregion
    }

    public interface IStateUI<S>
    {
        public S State { get; set; }
    }
}