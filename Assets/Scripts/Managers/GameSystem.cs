using System;
using System.Collections.Generic;
using System.Linq;
using FishNet;
using UnityEngine;
using FishNet.Connection;
using FishNet.Transporting;
using Tools;

public class GameSystem : MonoBehaviour
{
    [Header("REFERENCES")]
    [SerializeField] private NetServerCommunicate netServer;

    [Header("GAME STATUS")]
    [SerializeField] private List<GameChoice> playersChoices;
    [SerializeField] private List<PlayerHealth> playerHealths;
    [SerializeField] private int round = 0;
    //[SerializeField] private int inactiveRounds = 0;
    public int playersConnected => playerHealths?.Count ?? 0;

    [Header("LIFE VALUES")] 
    [SerializeField] private float maxHealth = 3;
    [SerializeField] private float roundDamage = 1;

    private const string winCode = "WIN";
    private const string loseCode = "LOSE";
    private const string drawCode = "DRAW";

    /*private float timer;
    private const int roundTimeLimit = 12;
    private const int inactiveRoundsLimit = 3;*/

    public GAME_STATE gameState;
    
    private const int oneCode = 0;
    private const int twoCode = 1;
    //private const float playerChoicesDelay = 2;

    #region Public methods

    public void ChangeGameState(GAME_STATE newState)
    {
        gameState = newState;
        CheckNewState();
    }

    public void RegisterNewPlayerConnection(int clientID, int objectID)
    {
        if (playerHealths == null)
            ClearGameMatch();

        var newPlayer = new PlayerHealth()
        {
            playerClientID = clientID,
            playerObjectID = objectID,
            playerHealth = maxHealth
        };
        playerHealths.Add(newPlayer);

        SendToClientHealthInit(newPlayer);

        if (playersConnected >= 2)
            ChangeGameState(GAME_STATE.STARTING);
    }
    
    public void RegisterPlayerChoice(int clientID, int objectID, int pChoice)
    {
        //var _pChoice = GetTypeFromInt(pChoice);
        var _pChoice = (CARD_TYPE)pChoice;

        foreach (var gameChoice in playersChoices)
        {
            if (gameChoice.playerObjectID != objectID || gameChoice.playerClientID != clientID) 
                continue;
            gameChoice.choice = _pChoice;
            break;
        }

        if (AllPlayersChose())
            //StartCoroutine(TimeTools.InvokeInTime(GoToCompareTime, playerChoicesDelay));
            GoToCompareTime();
    }

    public void ClientDisconnected(NetworkConnection conn, RemoteConnectionStateArgs args)
    {
        if (args.ConnectionState is RemoteConnectionState.Started)
            return;
        GameOverAllWin();
        var message = "ClientDisconnected_Args: args.connectionID: " + args.ConnectionId + ", args.connectionState: " 
                      + args.ConnectionState + "args.transportIndex: " + args.TransportIndex + 
                      " | NetConn.ClientID: " + conn.ClientId;
        SendStringMessageForAllClients(message);
    }

    #endregion
    
    #region Private Methods

    private void Start()
    {
        if (!InstanceFinder.IsServer)
            this.gameObject.GetComponent<GameSystem>().enabled = false;
        netServer ??= FindObjectOfType<NetServerCommunicate>();
        if(gameState == GAME_STATE.NONE)
            ChangeGameState(GAME_STATE.WAIT_CONNECTIONS);
    }

    private Match_Info GetMatchResult()
    {
        var matchResult = new Match_Info
        {
            choices = playersChoices,
            isDraw = playersChoices[oneCode].choice == playersChoices[twoCode].choice,
            winnerObjectID = -1,
            loserObjectID = -1,
            loserClientID = -1,
            winnerClientID = -1,
            //isInactivePlayers = playersChoices[oneCode].choice == CARD_TYPE.NONE && playersChoices[twoCode].choice == CARD_TYPE.NONE
        };

        if (matchResult.isDraw)
            return matchResult;

        var winnerID = -1;

        switch (playersChoices[oneCode].choice)
        {
            case CARD_TYPE.ROCK:
                winnerID = playersChoices[twoCode].choice switch
                {
                    CARD_TYPE.PAPER => twoCode,
                    CARD_TYPE.SCISSOR => oneCode,
                    CARD_TYPE.NONE => oneCode
                };
                break;
            case CARD_TYPE.PAPER:
                winnerID = playersChoices[twoCode].choice switch
                {
                    CARD_TYPE.SCISSOR => twoCode,
                    CARD_TYPE.ROCK => oneCode,
                    CARD_TYPE.NONE => oneCode
                };
                break;
            case CARD_TYPE.SCISSOR:
                winnerID = playersChoices[twoCode].choice switch
                {
                    CARD_TYPE.ROCK => twoCode,
                    CARD_TYPE.PAPER => oneCode,
                    CARD_TYPE.NONE => oneCode
                };
                break;
            case CARD_TYPE.NONE:
                if (playersChoices[twoCode].choice != CARD_TYPE.NONE)
                    winnerID = twoCode;
                break;
        }

        matchResult.winnerObjectID = playersChoices[winnerID].playerObjectID;
        matchResult.winnerClientID = playersChoices[winnerID].playerClientID;
        matchResult.loserClientID = winnerID == oneCode
            ? playersChoices[twoCode].playerClientID
            : playersChoices[oneCode].playerClientID;
        matchResult.loserObjectID = winnerID == oneCode
            ? playersChoices[twoCode].playerObjectID
            : playersChoices[oneCode].playerObjectID;
        return matchResult;
    }

    private CARD_TYPE GetTypeFromInt(int type)
    {
        return type switch
        {
            0 => CARD_TYPE.NONE,
            1 => CARD_TYPE.ROCK,
            2 => CARD_TYPE.PAPER,
            3 => CARD_TYPE.SCISSOR,
            _ => PlayerChoiceError()
        };
    }

    private CARD_TYPE PlayerChoiceError()
    {
        SendStringMessageForAllClients("[ERROR] Player Choice error, convert to NONE!");
        return CARD_TYPE.NONE;
    }

    private void CheckChoices()
    {
        var result = GetMatchResult();
        if (result.isDraw)
        {
            /*if (result.isInactivePlayers)
            {
                inactiveRounds++;
                if (inactiveRounds >= inactiveRoundsLimit)
                {
                    GameOverAllLose();
                    return;
                }
            }*/

            foreach (var gameChoice in playersChoices)
                SendToClientResult(gameChoice, drawCode);
            ClearChoices();
            ChangeGameState(GAME_STATE.CHOICE_TIME);
        }
        else
        {
            round++;
            DamagePlayer(result.loserObjectID, result.loserClientID);
            var playerDeath = GetPlayerDeath();
            if (playerDeath == null)
            {
                SendToClientResult(GetPlayerChoice(result.loserObjectID, result.loserClientID), loseCode);
                SendToClientResult(GetPlayerChoice(result.winnerObjectID, result.winnerClientID), winCode);
                ClearChoices();
                ChangeGameState(GAME_STATE.CHOICE_TIME);
            }
            else
                SendGameOverToClients();
        }
    }

    private void SendToClientResult(GameChoice playerChoice, string result)
    {
        var netMessage = new NetworkMessage
        {
            ClientID = playerChoice.playerClientID,
            ObjectID = playerChoice.playerObjectID,
            Content = result,
            MessageType = (int)MESSAGE_TYPE.ROUND_RESULT,
            ValueContent = GetPlayerHealth(playerChoice.playerObjectID, playerChoice.playerClientID)
        };
        netServer.SendMessageToClient(netMessage);
    }

    private void SendToClientHealthInit(PlayerHealth playerHealth)
    {
        var netMessage = new NetworkMessage
        {
            ClientID = playerHealth.playerClientID,
            ObjectID = playerHealth.playerObjectID,
            Content = "",
            MessageType = (int)MESSAGE_TYPE.NEW_CONNECTION,
            ValueContent = playerHealth.playerHealth
        };
        netServer.SendMessageToClient(netMessage);
    }

    private void SendGameOverToClients()
    {
        var playerDeath = GetPlayerDeath();
        var playerAlive = GetPlayerAlive();
        SendGameOverMessage(playerDeath, loseCode);
        SendGameOverMessage(playerAlive, winCode);
        ChangeGameState(GAME_STATE.GAME_OVER);
    }

    private void GameOverAllWin()
    {
        for(var i = 0; i < playerHealths.Count; i++)
            SendGameOverMessage(playerHealths[i], winCode);
        ChangeGameState(GAME_STATE.GAME_OVER);
    }
    
    private void GameOverAllLose()
    {
        for(var i = 0; i < playerHealths.Count; i++)
            SendGameOverMessage(playerHealths[i], loseCode);
        ChangeGameState(GAME_STATE.GAME_OVER);
    }

    private void SendGameOverMessage(PlayerHealth player, string endCode)
    {
        var gameOverMessage = new NetworkMessage()
        {
            ClientID = player.playerClientID,
            ObjectID = player.playerObjectID,
            Content = endCode,
            MessageType = (int)MESSAGE_TYPE.GAME_OVER,
            ValueContent = player.playerHealth
        };
        netServer.SendMessageToClient(gameOverMessage);
    }

    private PlayerHealth GetPlayerDeath()
    {
        return playerHealths.FirstOrDefault(player => player.playerHealth <= 0.1f);
    }
    private PlayerHealth GetPlayerAlive()
    {
        return playerHealths.FirstOrDefault(player => player.playerHealth >= 0);
    }

    private void DamagePlayer(int objectID, int clientID)
    {
        for (var i = 0; i < playerHealths.Count; i++)
        {
            if (playerHealths[i].playerObjectID == objectID 
                && playerHealths[i].playerClientID == clientID)
                playerHealths[i].playerHealth -= roundDamage;
        }
    }

    private float GetPlayerHealth(int objectID, int clientID)
    {
        float result = 0;
        foreach (var playerHealth in playerHealths)
        {
            if (playerHealth.playerObjectID == objectID 
                && playerHealth.playerClientID == clientID)
                result = playerHealth.playerHealth;
        }

        return result;
    }

    private void ClearGameMatch()
    {
        playerHealths = new List<PlayerHealth>();
        round = 0;
        /*inactiveRounds = 0;
        timer = 0;*/
        playersChoices?.Clear();
    }

    private GameChoice GetPlayerChoice(int objectID, int clientID)
        =>  playersChoices.FirstOrDefault(gameChoice => objectID == gameChoice.playerObjectID 
                                                        && clientID == gameChoice.playerClientID);

    private void SendStartGameForAllClients()
    {
        foreach (var player in playerHealths)
        {
            var netMessage = new NetworkMessage
            {
                ClientID = player.playerClientID,
                ObjectID = player.playerObjectID,
                Content = "",
                MessageType = (int)MESSAGE_TYPE.START_GAME,
                ValueContent = player.playerHealth
            };
            netServer.SendMessageToClient(netMessage);
        }
    }

    public void SendStringMessageForAllClients(string message)
    {
        foreach (var player in playerHealths)
        {
            var netMessage = new NetworkMessage
            {
                ClientID = player.playerClientID,
                ObjectID = player.playerObjectID,
                Content = message,
                MessageType = (int)MESSAGE_TYPE.STRING,
                ValueContent = player.playerHealth
            };
            netServer.SendMessageToClient(netMessage);
        }
    }

    private void SendGameSystemInfoToClients()
    {
        if (playerHealths.Count < 2 || playersChoices.Count < 2)
            return;
        var m = "playersChoices.Count: " + playersChoices.Count + " | round: " + round + 
                "\nPlayer[" + playerHealths[oneCode].playerObjectID + "] Health: " + playerHealths[oneCode].playerHealth +
                "\nPlayer[" + playerHealths[twoCode].playerObjectID + "] Health: " + playerHealths[twoCode].playerHealth;
        SendStringMessageForAllClients(m);
    }

    private PlayerHealth GetPlayerHealthByClientID(int id)
    {
        for (var i = 0; i < playerHealths.Count; i++)
        {
            if (playerHealths[i].playerClientID == id)
                return playerHealths[i];
        }

        return null;
    }

    /*private void Update()
    {
        if (gameState != GAME_STATE.CHOICE_TIME)
            return;
        timer += Time.deltaTime;
        if (timer >= roundTimeLimit)
            GoToCompareTime();
    }*/

    private void GoToCompareTime()
    {
        ChangeGameState(GAME_STATE.COMPARE_TIME);
        //timer = 0;
    }

    private void CheckNewState()
    {
        switch (gameState)
        {
            case GAME_STATE.WAIT_CONNECTIONS:
                break;
            case GAME_STATE.STARTING:
                StartMatch();
                break;
            case GAME_STATE.CHOICE_TIME:
                break;
            case GAME_STATE.COMPARE_TIME:
                CheckChoices();
                break;
            case GAME_STATE.GAME_OVER:
                ClearGameMatch();
                ChangeGameState(GAME_STATE.WAIT_CONNECTIONS);
                break;
        }
    }

    private void StartMatch()
    {
        ClearChoices();
        ChangeGameState(GAME_STATE.CHOICE_TIME);
        SendStartGameForAllClients();
    }

    private void ClearChoices()
    {
        playersChoices.Clear();
        playersChoices = new List<GameChoice>();
        for (var i = 0; i < playerHealths.Count; i++)
        {
            var item = new GameChoice
            {
                playerClientID = playerHealths[i].playerClientID,
                playerObjectID = playerHealths[i].playerObjectID,
                choice = CARD_TYPE.NONE
            };
            playersChoices.Add(item);
        }
    }

    private bool AllPlayersChose()
    {
        var result = true;
        foreach (var choice in playersChoices)
        {
            if (choice.choice == CARD_TYPE.NONE) 
                result = false;
        }

        return result;
    }

    #endregion
}

[Serializable]
public class GameChoice
{
    public int playerClientID;
    public int playerObjectID;
    public CARD_TYPE choice;
}

[Serializable]
public class PlayerHealth
{
    public int playerClientID;
    public int playerObjectID;
    public float playerHealth;
}