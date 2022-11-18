using System;
using UnityEngine;

public class PlayerInput : MonoBehaviour
{
    public bool buttonA;

    //private methods
    private void Update()
    {
        buttonA = Input.GetKeyDown(KeyCode.Space);
    }
}
