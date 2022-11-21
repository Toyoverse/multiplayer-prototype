using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerInput : MonoBehaviour
{
    [Header("TEST BUTTONS")]
    public bool spaceButton;
    public bool returnButton;

    [Header("CHOICE BUTTONS")]
    [SerializeField] private Button rockButton;
    [SerializeField] private Button paperButton;
    [SerializeField] private Button scissorButton;

    private const string rockCode = "ROCK";
    private const string paperCode = "PAPER";
    private const string scissorCode = "SCISSOR";
    private const string roundInitMessage = "Round started!\nChoice your move!";
    private const string choiceMessage = "You chose ";
    private const string opponentWait = "\nWaiting for the opponent's move...";

    private NetClientCommunicate myNetClientCommunicate;

    #region Public Methods

    public void PlayerInputsInit(NetClientCommunicate netClientCommunicate)
    {
        myNetClientCommunicate = netClientCommunicate;
        rockButton.onClick.AddListener(RockSelect);
        paperButton.onClick.AddListener(PaperSelect);
        scissorButton.onClick.AddListener(ScissorSelect);
        
        RoundInit();
    }

    public void RoundInit()
    {
        StartCoroutine(InvokeInTime(InitRound, 2));
    }
    
    #endregion
    
    #region Coroutines

    private IEnumerator InvokeInTime(Action method, float time)
    {
        yield return new WaitForSeconds(time);
        method?.Invoke();
    }

    #endregion
    
    #region Private Methods

    private void InitRound()
    {
        ShowLogs.Instance.Log(roundInitMessage);
        EnableButtons();
    }
    
    private void MoveSelect(string move)
    {
        myNetClientCommunicate.SendChoiceToServer(move);
        DisableButtons();
        ShowLogs.Instance.Log(choiceMessage + move + opponentWait);
    }

    private void RockSelect() => MoveSelect(rockCode);

    private void PaperSelect() => MoveSelect(paperCode);

    private void ScissorSelect() => MoveSelect(scissorCode);
    
    private void EnableButtons()
    {
        rockButton.interactable = true;
        paperButton.interactable = true;
        scissorButton.interactable = true;
    }

    private void DisableButtons()
    {
        rockButton.interactable = false;
        paperButton.interactable = false;
        scissorButton.interactable = false;
    }

    private void OnDisable()
    {
        rockButton.onClick.RemoveListener(RockSelect);
        paperButton.onClick.RemoveListener(PaperSelect);
        scissorButton.onClick.RemoveListener(ScissorSelect);
    }

    /*private void Update()
    {
        spaceButton = Input.GetKeyDown(KeyCode.Space);
        returnButton = Input.GetKeyDown(KeyCode.Return);
    }*/

    #endregion
}
