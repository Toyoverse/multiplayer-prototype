using UnityEngine;
using Tools;

public class LocalGameManager : MonoBehaviour
{
    private ScriptsReferences refs => ScriptsReferences.Instance;
    
    private const string rockCode = "ROCK";
    private const string paperCode = "PAPER";
    private const string scissorCode = "SCISSOR";
    private const string roundInitMessage = "Round started!\nMake your move!";
    private const string choiceMessage = "You chose ";
    private const string opponentWait = "\nWaiting for the opponent's move...";
    private const string successConnection = "Connection success!\nWaiting for an opponent...";
    private const string winMessage = "ROUND WIN!";
    private const string loseMessage = "ROUND LOSE!";
    private const string drawMessage = "DRAW!";
    private const string startingRoundMessage = "Starting a new round...";
    private const string waitMessage = "Please wait.";
    private const string matchWinMessage = "You WON the game!\nCongratulations!";
    private const string matchLoseMessage = "You lost the game. :(\nDon't get discouraged, try again!";
    private const string disconnectedMessage = "Disconnected from server.";
    private const string disconnectCountMessage = "You will be disconnected from the server in ";
    private const string genericErrorMessage = "Something went wrong, please try restarting the game.";

    public int round = 0;

    private string gameOverMessage = "";
    
    #region Events

    public delegate void StartRound();
    public StartRound onRoundStart;
    
    #endregion

    #region Public Methods

    public void ConnectionSuccess(float maxHealthValue)
    {
        ShowSimpleLogs.Instance.Log(successConnection);
        refs.myStatusManager.InitHealth(maxHealthValue);
    }
    
    public void GameInit()
    {
        refs.playerInput.rockButton.onClick.AddListener(RockSelect);
        refs.playerInput.paperButton.onClick.AddListener(PaperSelect);
        refs.playerInput.scissorButton.onClick.AddListener(ScissorSelect);
        refs.playerInput.menuButton.onClick.AddListener(BackToMenu);

        RoundInit();
    }
    
    public void RunRoundResult(string result, float healthValue)
    {
        switch (result.ToUpper())
        {
            case "WIN":
                ShowSimpleLogs.Instance.Log(winMessage + "\n" + startingRoundMessage + "\n" + waitMessage);
                break;
            case "DRAW":
                ShowSimpleLogs.Instance.Log(drawMessage + "\n" + startingRoundMessage + "\n" + waitMessage);
                break;
            case "LOSE":
                ShowSimpleLogs.Instance.Log(loseMessage + "\n" + startingRoundMessage + "\n" + waitMessage);
                break;
        }

        refs.myStatusManager.ChangeHealth(healthValue);
        RoundInit();
    }

    public void GameOver(string result)
    {
        gameOverMessage = result.ToUpper() switch
        {
            "WIN" => matchWinMessage,
            "LOSE" => matchLoseMessage,
            _ => genericErrorMessage
        };

        if(gameOverMessage == matchLoseMessage)
            refs.myStatusManager.ChangeHealth(0);
        
        CountToDisconnect(10);
    }
    
    public void DisconnectToServer() => refs.myNetHudCanvas.OnlyDisconnect();

    #endregion
    
    #region Private Methods

    private void BackToMenu()
    {
        DisconnectToServer();
        UIMenuManager.Instance.BackToMenu();
    }
    
    private void CountToDisconnect(float timeRemain)
    {
        if (timeRemain <= 0)
        {
            ShowSimpleLogs.Instance.Log(disconnectedMessage);
            DisconnectToServer();
            StartCoroutine(TimeTools.InvokeInTime(BackToMenu, 1));
        }
        else
        {
            ShowSimpleLogs.Instance.Log(gameOverMessage + "\n" + disconnectCountMessage + timeRemain);
            var newTimeRemain = timeRemain - 1;
            StartCoroutine(TimeTools.InvokeInTime(CountToDisconnect, newTimeRemain, 1));
        }
    }

    private void RoundInit() => StartCoroutine(TimeTools.InvokeInTime(InitRound, 2));

    private void InitRound()
    {
        UIMenuManager.Instance.GoToStartGame();
        if (refs.myStatusManager.health <= 0.1f)
            return;
        round++;
        ShowSimpleLogs.Instance.Log(roundInitMessage);
        refs.playerInput.EnableMoveButtons();
    }
    
    private void MoveSelect(string move)
    {
        refs.playerInput.DisableMoveButtons();
        
        var netMessage = new NetworkMessage()
        {
            ClientID = refs.myNetClientCommunicate.ClientManager.GetInstanceID(),
            ObjectID = refs.myNetClientCommunicate.NetworkObject.ObjectId,
            MessageType = (int)MESSAGE_TYPE.CARD_CHOICE,
            Content = move
        };
        refs.myNetClientCommunicate.SendMessageToServer(netMessage);
        
        ShowSimpleLogs.Instance.Log(choiceMessage + move + opponentWait);
    }

    private void RockSelect() => MoveSelect(rockCode);

    private void PaperSelect() => MoveSelect(paperCode);

    private void ScissorSelect() => MoveSelect(scissorCode);
    
    private void OnDisable()
    {
        if(refs is null)
            return;
        refs.playerInput.rockButton.onClick.RemoveListener(RockSelect);
        refs.playerInput.paperButton.onClick.RemoveListener(PaperSelect);
        refs.playerInput.scissorButton.onClick.RemoveListener(ScissorSelect);
    }

    #endregion
}
