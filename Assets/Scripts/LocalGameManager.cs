using UnityEngine;
using Tools;

public class LocalGameManager : MonoBehaviour
{
    private ScriptsReferences refs => ScriptsReferences.Instance;
    
    private const string rockCode = "ROCK";
    private const string paperCode = "PAPER";
    private const string scissorCode = "SCISSOR";
    private const string roundInitMessage = "Round started!\nChoice your move!";
    private const string choiceMessage = "You chose ";
    private const string opponentWait = "\nWaiting for the opponent's move...";
    private const string successConnection = "Connection success!\nWaiting for an opponent...";
    private const string winMessage = "ROUND WIN!";
    private const string loseMessage = "ROUND LOSE!";
    private const string drawMessage = "DRAW!";
    private const string startingRoundMessage = "Starting a new round...";
    private const string waitMessage = "Please wait.";

    public int round = 0;
    
    #region Events

    public delegate void StartRound();
    public StartRound onRoundStart;
    
    #endregion

    #region Public Methods

    public void ConnectionSuccess()
    {
        ShowLogs.Instance.Log(successConnection);
    }
    
    public void GameInit()
    {
        refs.playerInput.rockButton.onClick.AddListener(RockSelect);
        refs.playerInput.paperButton.onClick.AddListener(PaperSelect);
        refs.playerInput.scissorButton.onClick.AddListener(ScissorSelect);

        RoundInit();
    }
    
    public void RunRoundResult(string result, float healthValue)
    {
        switch (result.ToUpper())
        {
            case "WIN":
                ShowLogs.Instance.Log(winMessage + "\n" + startingRoundMessage + "\n" + waitMessage);
                break;
            case "DRAW":
                ShowLogs.Instance.Log(drawMessage + "\n" + startingRoundMessage + "\n" + waitMessage);
                break;
            case "LOSE":
                ShowLogs.Instance.Log(loseMessage + "\n" + startingRoundMessage + "\n" + waitMessage);
                break;
        }

        refs.myStatusManager.ChangeHealth(healthValue);
        RoundInit();
    }
    
    #endregion
    
    #region Private Methods
    
    private void RoundInit() => StartCoroutine(TimeTools.InvokeInTime(InitRound, 2));

    private void InitRound()
    {
        round++;
        ShowLogs.Instance.Log(roundInitMessage);
        refs.playerInput.EnableMoveButtons();
    }
    
    private void MoveSelect(string move)
    {
        var netMessage = new NetworkMessage()
        {
            ClientID = refs.myNetClientCommunicate.ClientManager.GetInstanceID(),
            ObjectID = refs.myNetClientCommunicate.NetworkObject.ObjectId,
            MessageType = (int)MESSAGE_TYPE.CARD_CHOICE,
            Content = move
        };
        refs.myNetClientCommunicate.SendMessageToServer(netMessage);
        
        refs.playerInput.DisableMoveButtons();
        ShowLogs.Instance.Log(choiceMessage + move + opponentWait);
    }

    private void RockSelect() => MoveSelect(rockCode);

    private void PaperSelect() => MoveSelect(paperCode);

    private void ScissorSelect() => MoveSelect(scissorCode);
    
    private void OnDisable()
    {
        refs.playerInput.rockButton.onClick.RemoveListener(RockSelect);
        refs.playerInput.paperButton.onClick.RemoveListener(PaperSelect);
        refs.playerInput.scissorButton.onClick.RemoveListener(ScissorSelect);
    }

    #endregion
}
