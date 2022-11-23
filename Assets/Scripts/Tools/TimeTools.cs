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

        #endregion
    }
}
