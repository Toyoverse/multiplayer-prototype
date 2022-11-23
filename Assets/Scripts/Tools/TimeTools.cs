using System;
using System.Collections;
using UnityEngine;

namespace Tools
{
    public static class TimeTools
    {
        #region Coroutines

        public static IEnumerator InvokeInTime(Action method, float time)
        {
            yield return new WaitForSeconds(time);
            method?.Invoke();
        }
        
        public static IEnumerator InvokeInTime(Action<float> method, float methodFloat, float time)
        {
            yield return new WaitForSeconds(time);
            method?.Invoke(methodFloat);
        }

        #endregion

        #region Public Methods

        #endregion

        #region Private Methods

        #endregion
    }
}
