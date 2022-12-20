using System.Collections;
using TMPro;
using UnityEngine;
using Tools;

public class LocalGameManager : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI roundText;
    private const string RoundTitle = "ROUND ";
    [SerializeField] private TextMeshProUGUI roundTimeText;
    [SerializeField] private GameObject roundTimeObj;

    private ScriptsReferences Refs => ScriptsReferences.Instance;

    [SerializeField] private int round;
    private string _gameOverMessage = "";
    public LOCAL_STATE gameState = LOCAL_STATE.MENU;
    
    //Temporary variables for post animation control
    private float _myHp, _opHp;
    public SIMPLE_RESULT myMatchResult = SIMPLE_RESULT.NONE;
    
    //Round time control
    [SerializeField] private float roundTime;
    private bool _moveReady;
    private CARD_TYPE _cardSelected = CARD_TYPE.EMPTY;

    #region Public Methods

    public void ChangeLocalState(LOCAL_STATE newState) => gameState = newState;
    
    public void ConnectionSuccess(float maxHealthValue)
    {
        ShowSimpleLogs.Instance.Log(Refs.globalConfig.successConnectionMessage);
        Refs.myStatusManager.InitHealth(maxHealthValue);
    }
    
    public void GameInit()
    {
        //ChangeLocalState(LOCAL_STATE.GAMEPLAY);
        RoundInit();
    }
    
    public void RunRoundResult(ServerNetMessage serverResult)
    {
        var log = Refs.globalConfig.opponentCardMessage + serverResult.opponentChoice + ".\n";
        var playableType = PLAYABLE_TYPE.NONE;
        switch (serverResult.roundResult)
        {
            case SIMPLE_RESULT.WIN:
                log += Refs.globalConfig.winMessage + "\n";
                playableType = PLAYABLE_TYPE.DAMAGE;
                break;
            case SIMPLE_RESULT.DRAW: 
                log += Refs.globalConfig.drawMessage + "\n";
                playableType = PLAYABLE_TYPE.DRAW;
                break;
            case SIMPLE_RESULT.LOSE:
                log += Refs.globalConfig.loseMessage + "\n";
                playableType = PLAYABLE_TYPE.KNOCKBACK;
                break;
        }
        
        log += Refs.globalConfig.startingRoundMessage + "\n" + Refs.globalConfig.waitMessage;
        Refs.timelineManager.PlayAnimation(playableType);
        ShowSimpleLogs.Instance.Log(log);
        _myHp = serverResult.playerHealth;
        _opHp = serverResult.opponentHealth;
    }

    public void OnEndRoundAnimation()
    {
        Refs.myStatusManager.ChangeHealth(_myHp);
        Refs.myStatusManager.ChangeOpHealth(_opHp);
        RoundInit();
    }

    public void OnEndMatchAnimation()
    {
        if(_gameOverMessage == Refs.globalConfig.matchLoseMessage)
            Refs.myStatusManager.ChangeHealth(0);
        else 
            Refs.myStatusManager.ChangeOpHealth(0);
        
        CountToDisconnect(_gameOverMessage + "\n" + Refs.globalConfig.disconnectCountMessage,
            Refs.globalConfig.endGameDisconnectDelay);
        myMatchResult = SIMPLE_RESULT.NONE;
    }

    public void GameOver(SIMPLE_RESULT result)
    {
        myMatchResult = result;
        _gameOverMessage = myMatchResult switch
        {
            SIMPLE_RESULT.WIN => Refs.globalConfig.matchWinMessage,
            SIMPLE_RESULT.LOSE => Refs.globalConfig.matchLoseMessage,
            _ => Refs.globalConfig.genericErrorMessage
        };

        var playableType = myMatchResult switch
        {
            SIMPLE_RESULT.WIN => PLAYABLE_TYPE.VICTORY,
            SIMPLE_RESULT.LOSE => PLAYABLE_TYPE.DEFEAT,
            _ => PLAYABLE_TYPE.NONE
        };
        
        Refs.timelineManager.PlayAnimation(playableType);
    }

    public void OnSelfKick() 
        => CountToDisconnect(Refs.globalConfig.disconnectedMessage + "\n" 
                             + Refs.globalConfig.backToMenuCountMessage, Refs.globalConfig.endGameDisconnectDelay);

    public void BackToMenu()
    {
        DisconnectToServer();
        UIMenuManager.Instance.BackToMenu();
    }
    
    #endregion
    
    #region Private Methods

    private void DisconnectToServer() => Refs.myNetHudCanvas.OnlyDisconnect();
    
    private void Start() => AddButtonEvents();

    private void CountToDisconnect(string defaultMessage, float timeRemain)
    {
        if (timeRemain <= 0)
        {
            ShowSimpleLogs.Instance.Log(Refs.globalConfig.disconnectedMessage);
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
        if (Refs.myStatusManager.health <= 0.1f)
            return;
        round++;
        roundText.text = RoundTitle + round;
        ShowSimpleLogs.Instance.Log(Refs.globalConfig.roundInitMessage);
        StartCoroutine(RoundCount());
    }
    
    private void MoveSelect(CARD_TYPE move)
    {
        _moveReady = true;
        _cardSelected = move;
    }

    private void SendMoveToServer()
    {
        var netMessage = JsonData.GetClientMessage(MESSAGE_TYPE.CARD_CHOICE, _cardSelected);
        Refs.myNetClientCommunicate.SendMessageToServer(netMessage);
    }

    private void RockSelect() => MoveSelect(CARD_TYPE.BOND);

    private void PaperSelect() => MoveSelect(CARD_TYPE.DEFENSE);

    private void ScissorSelect() => MoveSelect(CARD_TYPE.ATTACK);

    private void OnDisable() => RemoveButtonEvents();

    private void AddButtonEvents()
    {
        if(Refs is null)
            return;
        Refs.playerInput.rockButton.onClick.AddListener(RockSelect);
        Refs.playerInput.paperButton.onClick.AddListener(PaperSelect);
        Refs.playerInput.scissorButton.onClick.AddListener(ScissorSelect);
        Refs.playerInput.menuButton.onClick.AddListener(BackToMenu);
    }

    private void RemoveButtonEvents()
    {
        if(Refs is null)
            return;
        Refs.playerInput.rockButton.onClick.RemoveListener(RockSelect);
        Refs.playerInput.paperButton.onClick.RemoveListener(PaperSelect);
        Refs.playerInput.scissorButton.onClick.RemoveListener(ScissorSelect);
        Refs.playerInput.menuButton.onClick.RemoveListener(BackToMenu);
    }

    private IEnumerator RoundCount()
    {
        roundTime = Refs.globalConfig.secondsPerRound;
        _moveReady = false;
        _cardSelected = CARD_TYPE.EMPTY;
        roundTimeText.text = "" + roundTime;
        roundTimeObj.SetActive(true);
        Refs.playerInput.EnableMoveButtons();
        
        while (roundTime >= 0)
        {
            if (myMatchResult != SIMPLE_RESULT.NONE)
                roundTime = -1;
            
            yield return new WaitForSeconds(1);
            if (!_moveReady)
            {
                roundTime--;
                roundTimeText.text = "" + roundTime;
            }
            else
            {
                roundTime = -1;
                roundTimeText.text = Refs.globalConfig.choiceMessage + _cardSelected;
            }
        }
        
        Refs.playerInput.DisableMoveButtons();
        if (myMatchResult != SIMPLE_RESULT.NONE)
        {
            roundTimeObj.SetActive(false);
            roundTimeText.text = "";
            yield break;
        }
        if (!_moveReady)
        {
            MoveSelect(CARD_TYPE.NONE);
            roundTimeText.text = Refs.globalConfig.roundTimeEndMessage;
        }
        yield return new WaitForSeconds(1);
        
        ShowSimpleLogs.Instance.Log(Refs.globalConfig.choiceMessage + _cardSelected 
                                    + Refs.globalConfig.opponentWaitMessage);
        roundTimeObj.SetActive(false);
        roundTimeText.text = "";
        
        SendMoveToServer();
    }

    #endregion
}
