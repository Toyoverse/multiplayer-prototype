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

    [Header("GAME BUTTONS")] 
    public Button menuButton;

    [Header("CHEAT")]
    private List<string> cheatKeyList;
    private int cheatIndex = 0;
    private ScriptsReferences refs => ScriptsReferences.Instance;

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

    private void SetCheatKeyList()
    {
        cheatKeyList = new List<string>();
        for (var i = 0; i < refs.globalConfig.lanCheat.Length; i++)
            cheatKeyList.Add(refs.globalConfig.lanCheat[i].ToString());
    }

    private void CheatSuccess()
    {
        Debug.LogError("CHEAT SUCCESS!");
        var canvas = refs.myNetHudCanvas.GetComponent<Canvas>();
        canvas.enabled = !canvas.isActiveAndEnabled;
    }

    private void Start()
    {
        SetCheatKeyList();
    }

    private void Update()
    {
        CheatCheck();
    }

    private void CheatCheck()
    {
        if (Input.anyKeyDown) 
        {
            if (Input.GetKeyDown(cheatKeyList[cheatIndex]))
                cheatIndex++;
            else 
                cheatIndex = 0;    
        }

        if (cheatIndex == refs.globalConfig.lanCheat.Length)
        {
            cheatIndex = 0;
            CheatSuccess();
        }
    }

    #endregion
}
