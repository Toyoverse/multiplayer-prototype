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
    [SerializeField] private List<PlayerClient> playerClients;
    [SerializeField] private int round = 0;
    [SerializeField] private int inactiveRounds = 0;
    public int playersConnected => playerClients?.Count ?? 0;

    [Header("LIFE VALUES")] 
    [SerializeField] private float maxHealth = 4;
    [SerializeField] private float roundDamage = 1;

    private const SIMPLE_RESULT winCode = SIMPLE_RESULT.WIN;
    private const SIMPLE_RESULT loseCode = SIMPLE_RESULT.LOSE;
    private const SIMPLE_RESULT drawCode = SIMPLE_RESULT.DRAW;
    
    private const int inactiveRoundsLimit = 5;

    public SERVER_STATE serverState;
    
    private const int oneCode = 0;
    private const int twoCode = 1;
    private const float playerChoicesDelay = 2;
    private const float gameOverDelay = 5;

    #region Public methods

    public void ChangeGameState(SERVER_STATE newState)
    {
        serverState = newState;
        CheckNewState();
    }

    public void RegisterNewPlayerConnection(int clientID, int objectID, NetworkConnection conn)
    {
        if (playerClients == null)
            ClearGameMatch();

        var newPlayer = new PlayerClient()
        {
            playerClientID = clientID,
            playerObjectID = objectID,
            playerHealth = maxHealth,
            networkConnection = conn
        };
        playerClients.Add(newPlayer);

        SendToClientHealthInit(newPlayer);

        if (playersConnected >= 2)
            ChangeGameState(SERVER_STATE.STARTING);
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
            StartCoroutine(TimeTools.InvokeInTime(GoToCompareTime, playerChoicesDelay));
            //GoToCompareTime();
    }

    public void ClientDisconnected(NetworkConnection conn, RemoteConnectionStateArgs args)
    {
        if (serverState is SERVER_STATE.GAME_OVER || args.ConnectionState is RemoteConnectionState.Started)
            return;
        GameOverWoWin();
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
        if(serverState == SERVER_STATE.NONE)
            ChangeGameState(SERVER_STATE.WAIT_CONNECTIONS);
    }

    private Match_Info GetMatchResult()
    {
        var matchResult = new Match_Info
        {
            isDraw = playersChoices[oneCode].choice == playersChoices[twoCode].choice,
            winnerChoice = new GameChoice(),
            loserChoice = new GameChoice()
            //isInactivePlayers = playersChoices[oneCode].choice == CARD_TYPE.NONE && playersChoices[twoCode].choice == CARD_TYPE.NONE
        };

        if (matchResult.isDraw)
            return matchResult;

        var winnerID = GetWinnerCode(false);
        matchResult.winnerChoice = playersChoices[winnerID];
        matchResult.loserChoice = winnerID == oneCode ? playersChoices[twoCode] : playersChoices[oneCode];
        
        return matchResult;
    }

    private int GetWinnerCode(bool isDraw)
    {
        if (isDraw)
            return -1;
        
        if (playersChoices[oneCode].choice == CARD_TYPE.NONE)
            return twoCode;

        var pTwoLose = PerkSystem.GetWeakestType(playersChoices[oneCode].choice);
        if (playersChoices[twoCode].choice == pTwoLose
            || playersChoices[twoCode].choice == CARD_TYPE.NONE)
            return oneCode;
        else
            return twoCode;
    }

    private CARD_TYPE GetTypeFromInt(int type)
    {
        return type switch
        {
            0 => CARD_TYPE.NONE,
            1 => CARD_TYPE.BOND,
            2 => CARD_TYPE.DEFENSE,
            3 => CARD_TYPE.ATTACK,
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
        round++;
        var result = GetMatchResult();
        if (IsLastInactiveRound())
            return;
        if (result.isDraw)
        {
            foreach (var player in playerClients)
            {
                var opponent = GetOpponent(player);
                SendToClientResult(GetPlayerChoice(player.playerObjectID, player.playerClientID), drawCode, 
                    GetPlayerChoice(opponent.playerObjectID, opponent.playerClientID));
            }
            ClearChoices();
            ChangeGameState(SERVER_STATE.CHOICE_TIME);
        }
        else
        {
            DamagePlayer(result.loserChoice);
            var playerDeath = GetPlayerDeath();
            if (playerDeath == null)
            {
                SendToClientResult(result.loserChoice, loseCode, result.winnerChoice);
                SendToClientResult(result.winnerChoice, winCode, result.loserChoice);
                ClearChoices();
                ChangeGameState(SERVER_STATE.CHOICE_TIME);
            }
            else
                SendGameOverToClients(playerDeath);
        }
    }

    private bool IsLastInactiveRound()
    {
        var thisRoundInactive = true;
        foreach (var pChoice in playersChoices)
        {
            if (pChoice.choice != CARD_TYPE.NONE)
                thisRoundInactive = false;
        }

        inactiveRounds = thisRoundInactive ? inactiveRounds + 1 : 0;
        if (inactiveRounds >= inactiveRoundsLimit)
        {
            KickAllForInactivity();
            return true;
        }
        else
            return false;
    }

    private void SendToClientResult(GameChoice playerChoice, SIMPLE_RESULT result, GameChoice opponentChoice)
    {
        var stringContent = result + "/" + opponentChoice.choice;
        var netMessage = new NetworkMessage
        {
            ClientID = playerChoice.playerClientID,
            ObjectID = playerChoice.playerObjectID,
            StringContent = stringContent,
            MessageType = (int)MESSAGE_TYPE.ROUND_RESULT,
            ValueOneContent = GetPlayerHealth(playerChoice.playerObjectID, playerChoice.playerClientID),
            ValueTwoContent = GetPlayerHealth(opponentChoice.playerObjectID, opponentChoice.playerClientID)
        };
        netServer.SendMessageToClient(netMessage);
    }

    private void SendToClientHealthInit(PlayerClient playerClient)
    {
        var netMessage = new NetworkMessage
        {
            ClientID = playerClient.playerClientID,
            ObjectID = playerClient.playerObjectID,
            StringContent = "",
            MessageType = (int)MESSAGE_TYPE.NEW_CONNECTION,
            ValueOneContent = playerClient.playerHealth,
            ValueTwoContent = playerClient.playerHealth
        };
        netServer.SendMessageToClient(netMessage);
    }

    private void SendGameOverToClients(PlayerClient loser)
    {
        var winner = GetOpponent(loser);
        SendGameOverMessage(loser, loseCode);
        SendGameOverMessage(winner, winCode);
        ChangeGameState(SERVER_STATE.GAME_OVER);
    }

    private void GameOverWoWin()
    {
        for(var i = 0; i < playerClients.Count; i++)
            if(playerClients[i].networkConnection.IsActive)
                SendGameOverMessage(playerClients[i], winCode);
        ChangeGameState(SERVER_STATE.GAME_OVER);
    }
    
    private void KickAllForInactivity()
    {
        for(var i = 0; i < playerClients.Count; i++)
            if(playerClients[i].networkConnection.IsActive)
                //SendGameOverMessage(playerClients[i], loseCode);
                netServer.KickPlayer(playerClients[i], KICK_REASON.INACTIVE);
        ChangeGameState(SERVER_STATE.GAME_OVER);
    }
    
    private void SendGameOverMessage(PlayerClient player, SIMPLE_RESULT endCode)
    {
        var gameOverMessage = new NetworkMessage()
        {
            ClientID = player.playerClientID,
            ObjectID = player.playerObjectID,
            StringContent = endCode.ToString(),
            MessageType = (int)MESSAGE_TYPE.GAME_OVER,
            ValueOneContent = player.playerHealth,
            ValueTwoContent = endCode == SIMPLE_RESULT.WIN ? 0 : -1
        };
        netServer.SendMessageToClient(gameOverMessage);
    }

    private PlayerClient GetPlayerDeath() => playerClients.FirstOrDefault(player => player.playerHealth <= 0.1f);
    
    private PlayerClient GetPlayerAlive() => playerClients.FirstOrDefault(player => player.playerHealth >= 0);

    private PlayerClient GetOpponent(PlayerClient myPlayer)
    {
        foreach (var player in playerClients)
        {
            if (player.playerClientID == myPlayer.playerClientID 
                && player.playerObjectID == myPlayer.playerObjectID)
                continue;
            return player;
        }
        return null;
    }

    private void DamagePlayer(GameChoice gameChoice)
    {
        for (var i = 0; i < playerClients.Count; i++)
        {
            if (playerClients[i].playerObjectID == gameChoice.playerObjectID 
                && playerClients[i].playerClientID == gameChoice.playerClientID)
                playerClients[i].playerHealth -= roundDamage;
        }
    }

    private float GetPlayerHealth(int objectID, int clientID)
    {
        float result = 0;
        foreach (var playerHealth in playerClients)
        {
            if (playerHealth.playerObjectID == objectID 
                && playerHealth.playerClientID == clientID)
                result = playerHealth.playerHealth;
        }

        return result;
    }

    private void ClearGameMatch()
    {
        round = 0;
        inactiveRounds = 0;
        playersChoices?.Clear();
        if (serverState is SERVER_STATE.GAME_OVER)
        {
            for(var i = 0; i < playerClients.Count; i++)
                if(playerClients[i].networkConnection.IsActive)
                    playerClients[i].networkConnection.Disconnect(true);
            ChangeGameState(SERVER_STATE.WAIT_CONNECTIONS);
        }
        playerClients?.Clear();
        playerClients = new List<PlayerClient>();
    }

    private GameChoice GetPlayerChoice(int objectID, int clientID)
        =>  playersChoices.FirstOrDefault(gameChoice => objectID == gameChoice.playerObjectID 
                                                        && clientID == gameChoice.playerClientID);

    private void SendStartGameForAllClients()
    {
        foreach (var player in playerClients)
        {
            var netMessage = new NetworkMessage
            {
                ClientID = player.playerClientID,
                ObjectID = player.playerObjectID,
                StringContent = "",
                MessageType = (int)MESSAGE_TYPE.START_GAME,
                ValueOneContent = player.playerHealth
            };
            netServer.SendMessageToClient(netMessage);
        }
    }

    public void SendStringMessageForAllClients(string message)
    {
        foreach (var player in playerClients)
        {
            var netMessage = new NetworkMessage
            {
                ClientID = player.playerClientID,
                ObjectID = player.playerObjectID,
                StringContent = message,
                MessageType = (int)MESSAGE_TYPE.STRING,
                ValueOneContent = player.playerHealth
            };
            netServer.SendMessageToClient(netMessage);
        }
    }

    private void SendGameSystemInfoToClients()
    {
        if (playerClients.Count < 2 || playersChoices.Count < 2)
            return;
        var m = "playersChoices.Count: " + playersChoices.Count + " | round: " + round + 
                "\nPlayer[" + playerClients[oneCode].playerObjectID + "] Health: " + playerClients[oneCode].playerHealth +
                "\nPlayer[" + playerClients[twoCode].playerObjectID + "] Health: " + playerClients[twoCode].playerHealth;
        SendStringMessageForAllClients(m);
    }

    private PlayerClient GetPlayerHealthByClientID(int id)
    {
        for (var i = 0; i < playerClients.Count; i++)
        {
            if (playerClients[i].playerClientID == id)
                return playerClients[i];
        }

        return null;
    }

    private void GoToCompareTime() => ChangeGameState(SERVER_STATE.COMPARE_TIME);

    private void CheckNewState()
    {
        switch (serverState)
        {
            case SERVER_STATE.WAIT_CONNECTIONS:
                break;
            case SERVER_STATE.STARTING:
                StartMatch();
                break;
            case SERVER_STATE.CHOICE_TIME:
                break;
            case SERVER_STATE.COMPARE_TIME:
                CheckChoices();
                break;
            case SERVER_STATE.GAME_OVER:
                StartCoroutine(TimeTools.InvokeInTime(ClearGameMatch, gameOverDelay));
                break;
        }
    }

    private void StartMatch()
    {
        ClearChoices();
        ChangeGameState(SERVER_STATE.CHOICE_TIME);
        SendStartGameForAllClients();
    }

    private void ClearChoices()
    {
        playersChoices.Clear();
        playersChoices = new List<GameChoice>();
        for (var i = 0; i < playerClients.Count; i++)
        {
            var item = new GameChoice
            {
                playerClientID = playerClients[i].playerClientID,
                playerObjectID = playerClients[i].playerObjectID,
                choice = CARD_TYPE.EMPTY
            };
            playersChoices.Add(item);
        }
    }

    private bool AllPlayersChose()
    {
        var result = true;
        foreach (var choice in playersChoices)
        {
            if (choice.choice == CARD_TYPE.EMPTY) 
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
public class PlayerClient
{
    public int playerClientID;
    public int playerObjectID;
    public float playerHealth;
    public NetworkConnection networkConnection;
}