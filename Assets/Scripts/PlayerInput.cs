using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Tools;

public class PlayerInput : MonoBehaviour
{
    [Header("CHOICE BUTTONS")]
    public Button rockButton;
    public Button paperButton;
    public Button scissorButton;

    #region Public Methods

    public void EnableMoveButtons() => MoveButtonsInteractable(true);

    public void DisableMoveButtons() => MoveButtonsInteractable(false);
    
    #endregion

    #region Private Methods

    private void MoveButtonsInteractable(bool on)
    {
        rockButton.interactable = on;
        paperButton.interactable = on;
        scissorButton.interactable = on;
    }

    #endregion
}
