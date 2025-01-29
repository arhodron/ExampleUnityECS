using UnityEngine;
using System.Collections.Generic;
using System;
using UnityEngine.UI;
using UnityEngine.EventSystems;

namespace Game.UI
{
    public class ClickStateUIMono : StateUIMono<bool>, IPointerDownHandler, IPointerUpHandler
    {
        #region IPointerDownHandler

        void IPointerDownHandler.OnPointerDown(PointerEventData eventData)
        {
            State = true;
        }

        #endregion

        #region IPointerUpHandler

        void IPointerUpHandler.OnPointerUp(PointerEventData eventData)
        {
            State = false;
        }

        #endregion
    }
}