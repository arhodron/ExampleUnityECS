using UnityEngine;
using System.Collections.Generic;
using System;
using UnityEngine.UI;
using UnityEngine.EventSystems;

namespace Game.UI
{
    [RequireComponent(typeof(Toggle))]
    public class ToggleStateUIMono : StateUIMono<bool>
    {
        [NonSerialized]
        private Toggle toggle = null;

        protected override void AwakeState()
        {
            toggle = GetComponent<Toggle>();
        }

        protected override void OnEnableState()
        {
            State = toggle.isOn;
        }

        public void OnTooggle(bool enabled)
        {
            State = enabled;
        }

        protected override void OnChangeState(bool current, bool last)
        {
            toggle.isOn = current;
        }
    }
}