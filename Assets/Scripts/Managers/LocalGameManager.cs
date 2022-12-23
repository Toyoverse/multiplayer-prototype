using System;
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
    private int _myCombo, _opCombo;
    
    //Round time control
    [SerializeField] private float roundTime;
    private bool _moveReady;
    private CARD_TYPE _cardSelected = CARD_TYPE.EMPTY;
    private int _cardSelectedAmount;

    #region Public Methods

    public void ChangeLocalState(LOCAL_STATE newState) => gameState = newState;
    
    public void ConnectionSuccess(float maxHealthValue)
    {
        ShowSimpleLogs.Instance.Log(Refs.globalConfig.successConnectionMessage);
        Refs.myStatusManager.InitHealth(maxHealthValue);
    }
    
    public void GameInit()
    {
        round = 0;
        UIMenuManager.Instance.GoToStartGame();
        Refs.timelineManager.PlayCardsAnimation();
        StartCoroutine(TimeTools.InvokeInTime(InitRound, 2));
    }

    public void RunRoundResult(ServerNetMessage serverResult)
    {
        LogRoundResult(serverResult);
        SetStatsRoundResult(serverResult);
        Refs.myStatusManager.UpdateMyCombo(_myCombo);
        Refs.myStatusManager.UpdateOpponentCombo(_opCombo);
        PlayRoundAnimationResult(serverResult);
    }

    public void OnEndRoundAnimation()
    {
        Refs.myStatusManager.ChangeHealth(_myHp);
        Refs.myStatusManager.ChangeOpHealth(_opHp);
        StartCoroutine(TimeTools.InvokeInTime(InitRound, 2));
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
        
        Refs.timelineManager.PlayToyoAnimation(playableType);
    }

    public void OnSelfKick() 
        => CountToDisconnect(Refs.globalConfig.disconnectedMessage + "\n" 
                             + Refs.globalConfig.backToMenuCountMessage, Refs.globalConfig.endGameDisconnectDelay);

    public void BackToMenu()
    {
        Refs.handManager.DisableCardsAmountUI();
        DisconnectToServer();
        UIMenuManager.Instance.BackToMenu();
        Refs.timelineManager.PlayToyoAnimation(PLAYABLE_TYPE.IDLE);
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

    private void InitRound()
    {
        if (round == 0)
            Refs.handManager?.BuyInitialHand();
        else
            Refs.handManager.BuyRoundCards();
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
        _cardSelectedAmount = Refs.handManager.DiscardCardsByType(move);
    }

    private void SendMoveToServer()
    {
        var netMessage = JsonData.GetClientMessage(MESSAGE_TYPE.CARD_CHOICE, _cardSelected, _cardSelectedAmount);
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

    private void LogRoundResult(ServerNetMessage serverResult)
    {
        var log = Refs.globalConfig.opponentCardMessage + serverResult.opponentChoice + ".\n";
        log += serverResult.roundResult switch
        {
            SIMPLE_RESULT.WIN => Refs.globalConfig.winMessage + "\n",
            SIMPLE_RESULT.DRAW => Refs.globalConfig.drawMessage + "\n",
            SIMPLE_RESULT.LOSE => Refs.globalConfig.loseMessage + "\n"
        };
        log += Refs.globalConfig.startingRoundMessage + "\n" + Refs.globalConfig.waitMessage;
        ShowSimpleLogs.Instance.Log(log);
    }

    private void PlayRoundAnimationResult(ServerNetMessage serverResult)
    {
        var playableType = serverResult.roundResult switch
        {
            SIMPLE_RESULT.WIN => PLAYABLE_TYPE.DAMAGE,
            SIMPLE_RESULT.DRAW => PLAYABLE_TYPE.DRAW,
            SIMPLE_RESULT.LOSE => PLAYABLE_TYPE.KNOCKBACK
        };
        Refs.timelineManager.PlayToyoAnimation(playableType);
    }

    private void SetStatsRoundResult(ServerNetMessage serverResult)
    {
        _myHp = serverResult.playerHealth;
        _opHp = serverResult.opponentHealth;
        _myCombo = serverResult.playerCombo;
        _opCombo = serverResult.opponentCombo;
    }

    #endregion
}
