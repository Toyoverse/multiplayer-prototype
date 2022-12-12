using TMPro;
using UnityEngine;
using Tools;

public class LocalGameManager : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI roundText;
    private const string roundTitle = "ROUND ";
    
    private ScriptsReferences refs => ScriptsReferences.Instance;
    
    private const string rockCode = "BOUND";
    private const string paperCode = "DEFENSE";
    private const string scissorCode = "FAST-ATTACK";
    public const string roundInitMessage = "Round started!\nMake your move!";
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
    private const string disconnectedMessage = "You have been disconnected from the server.";
    private const string disconnectCountMessage = "You will be disconnected from the server in ";
    private const string genericErrorMessage = "Something went wrong, please try restarting the game.";
    private const string opponentCardMessage = "Your opponent chose ";
    private const string backToMenuInMenssage = "Back to menu in ";

    public int round = 0;

    private string gameOverMessage = "";
    private int endGameDisconnectDelay = 5;

    public LOCAL_STATE gameState = LOCAL_STATE.MENU;
    
    //Temporary variables for post animation control
    private float myHp, opHp;
    public SIMPLE_RESULT myMatchResult = SIMPLE_RESULT.NONE;

    #region Public Methods

    public void ChangeLocalState(LOCAL_STATE newState) => gameState = newState;
    
    public void ConnectionSuccess(float maxHealthValue)
    {
        ShowSimpleLogs.Instance.Log(successConnection);
        refs.myStatusManager.InitHealth(maxHealthValue);
    }
    
    public void GameInit()
    {
        //ChangeLocalState(LOCAL_STATE.GAMEPLAY);
        RoundInit();
    }
    
    public void RunRoundResult(string result, float healthValue, float opponentHp)
    {
        var resultSplit = result.Split("/");
        var simpleResult = GetResultFromString(resultSplit[0].ToUpper());
        var log = opponentCardMessage + resultSplit[1] + ".\n";
        var playableType = PLAYABLE_TYPE.NONE;
        switch (simpleResult)
        {
            case SIMPLE_RESULT.WIN:
                log += winMessage + "\n";
                playableType = PLAYABLE_TYPE.DAMAGE;
                break;
            case SIMPLE_RESULT.DRAW: 
                log += drawMessage + "\n";
                playableType = PLAYABLE_TYPE.DRAW;
                break;
            case SIMPLE_RESULT.LOSE:
                log += loseMessage + "\n";
                playableType = PLAYABLE_TYPE.KNOCKBACK;
                break;
        }
        
        log += startingRoundMessage + "\n" + waitMessage;
        refs.timelineManager.PlayAnimation(playableType);
        ShowSimpleLogs.Instance.Log(log);
        myHp = healthValue;
        opHp = opponentHp;
    }

    public void OnEndRoundAnimation()
    {
        refs.myStatusManager.ChangeHealth(myHp);
        refs.myStatusManager.ChangeOpHealth(opHp);
        RoundInit();
    }

    public void OnEndMatchAnimation()
    {
        if(gameOverMessage == matchLoseMessage)
            refs.myStatusManager.ChangeHealth(0);
        else 
            refs.myStatusManager.ChangeOpHealth(0);
        
        CountToDisconnect(gameOverMessage + "\n" + disconnectCountMessage, endGameDisconnectDelay);
        myMatchResult = SIMPLE_RESULT.NONE;
    }

    public void GameOver(string result)
    {
        myMatchResult = GetResultFromString(result);
        gameOverMessage = myMatchResult switch
        {
            SIMPLE_RESULT.WIN => matchWinMessage,
            SIMPLE_RESULT.LOSE => matchLoseMessage,
            _ => genericErrorMessage
        };

        var playableType = myMatchResult switch
        {
            SIMPLE_RESULT.WIN => PLAYABLE_TYPE.VICTORY,
            SIMPLE_RESULT.LOSE => PLAYABLE_TYPE.DEFEAT,
            _ => PLAYABLE_TYPE.NONE
        };
        
        refs.timelineManager.PlayAnimation(playableType);
    }
    
    public void DisconnectToServer() => refs.myNetHudCanvas.OnlyDisconnect();

    public void OnSelfKick() 
        => CountToDisconnect(disconnectedMessage + "\n" + backToMenuInMenssage, endGameDisconnectDelay);

    #endregion
    
    #region Private Methods

    private void Start() => AddButtonEvents();

    private void BackToMenu()
    {
        DisconnectToServer();
        UIMenuManager.Instance.BackToMenu();
    }
    
    private void CountToDisconnect(string defaultMessage, float timeRemain)
    {
        if (timeRemain <= 0)
        {
            ShowSimpleLogs.Instance.Log(disconnectedMessage);
            if(gameState != LOCAL_STATE.MENU)
                StartCoroutine(TimeTools.InvokeInTime(BackToMenu, 1));
        }
        else
        {
            ShowSimpleLogs.Instance.Log(defaultMessage + timeRemain);
            var newTimeRemain = timeRemain - 1;
            StartCoroutine(TimeTools.InvokeInTime(CountToDisconnect, defaultMessage, newTimeRemain, 1));
        }
    }

    private void RoundInit() => StartCoroutine(TimeTools.InvokeInTime(InitRound, 2));

    private void InitRound()
    {
        if (gameState != LOCAL_STATE.GAMEPLAY)
        {
            round = 0;
            UIMenuManager.Instance.GoToStartGame();
        }
        if (refs.myStatusManager.health <= 0.1f)
            return;
        round++;
        roundText.text = roundTitle + round;
        ShowSimpleLogs.Instance.Log(roundInitMessage);
        refs.playerInput.EnableMoveButtons();
    }
    
    private void MoveSelect(CARD_TYPE move)
    {
        refs.playerInput.DisableMoveButtons();
        
        var netMessage = new NetworkMessage()
        {
            ClientID = refs.myNetClientCommunicate.ClientManager.GetInstanceID(),
            ObjectID = refs.myNetClientCommunicate.NetworkObject.ObjectId,
            MessageType = (int)MESSAGE_TYPE.CARD_CHOICE,
            StringContent = ((int)move).ToString()
        };
        refs.myNetClientCommunicate.SendMessageToServer(netMessage);
        
        ShowSimpleLogs.Instance.Log(choiceMessage + move + opponentWait);
    }

    private void RockSelect() => MoveSelect(CARD_TYPE.BOND);

    private void PaperSelect() => MoveSelect(CARD_TYPE.DEFENSE);

    private void ScissorSelect() => MoveSelect(CARD_TYPE.ATTACK);

    private void OnDisable() => RemoveButtonEvents();

    private void AddButtonEvents()
    {
        if(refs is null)
            return;
        refs.playerInput.rockButton.onClick.AddListener(RockSelect);
        refs.playerInput.paperButton.onClick.AddListener(PaperSelect);
        refs.playerInput.scissorButton.onClick.AddListener(ScissorSelect);
        refs.playerInput.menuButton.onClick.AddListener(BackToMenu);
    }

    private void RemoveButtonEvents()
    {
        if(refs is null)
            return;
        refs.playerInput.rockButton.onClick.RemoveListener(RockSelect);
        refs.playerInput.paperButton.onClick.RemoveListener(PaperSelect);
        refs.playerInput.scissorButton.onClick.RemoveListener(ScissorSelect);
        refs.playerInput.menuButton.onClick.RemoveListener(BackToMenu);
    }

    private SIMPLE_RESULT GetResultFromString(string stringResult)
        => stringResult.ToUpper() switch
            {
                "WIN" => SIMPLE_RESULT.WIN,
                "LOSE" => SIMPLE_RESULT.LOSE,
                "DRAW" => SIMPLE_RESULT.DRAW,
                _ => SIMPLE_RESULT.NONE
            };

    #endregion
}
