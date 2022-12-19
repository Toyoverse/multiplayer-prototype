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
    [SerializeField] private ScriptsReferences refs => ScriptsReferences.Instance;

    [Header("GAME STATUS")]
    [SerializeField] private List<PlayerClient> playerClients;
    [SerializeField] private int round = 0;
    [SerializeField] private int inactiveRounds = 0;
    public int PlayersConnected => playerClients?.Count ?? 0;

    private const SIMPLE_RESULT winCode = SIMPLE_RESULT.WIN;
    private const SIMPLE_RESULT loseCode = SIMPLE_RESULT.LOSE;
    private const SIMPLE_RESULT drawCode = SIMPLE_RESULT.DRAW;

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
            playerHealth = refs.globalConfig.maxHealth,
            networkConnection = conn,
            choice = CARD_TYPE.EMPTY,
            combo = -1
        };
        playerClients.Add(newPlayer);

        SendToClientHealthInit(newPlayer);

        if (PlayersConnected >= 2)
            ChangeGameState(SERVER_STATE.STARTING);
    }
    
    public void RegisterPlayerChoice(int clientID, int objectID, int pChoice)
    {
        var _pChoice = (CARD_TYPE)pChoice;

        foreach (var playerClient in playerClients)
        {
            if (playerClient.playerObjectID != objectID || playerClient.playerClientID != clientID) 
                continue;
            playerClient.choice = _pChoice;
            break;
        }

        if (AllPlayersChose())
            StartCoroutine(TimeTools.InvokeInTime(GoToCompareTime, playerChoicesDelay));
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
        {
            this.gameObject.GetComponent<GameSystem>().enabled = false;
            return;
        }
        netServer ??= FindObjectOfType<NetServerCommunicate>();
        if(serverState == SERVER_STATE.NONE)
            ChangeGameState(SERVER_STATE.WAIT_CONNECTIONS);
    }

    private Match_Info GetMatchResult()
    {
        var matchResult = new Match_Info
        {
            isDraw = playerClients[oneCode].choice == playerClients[twoCode].choice,
            winnerClient = new PlayerClient(),
            loserClient = new PlayerClient()
        };

        if (matchResult.isDraw)
        {
            ApplyCombos(playerClients[oneCode], playerClients[twoCode], true);
            return matchResult;
        }

        var winnerID = GetWinnerCode(false);
        matchResult.winnerClient = playerClients[winnerID];
        matchResult.loserClient = winnerID == oneCode ? playerClients[twoCode] : playerClients[oneCode];
        ApplyCombos(matchResult.winnerClient, matchResult.loserClient);
        
        return matchResult;
    }

    private int GetWinnerCode(bool isDraw)
    {
        if (isDraw)
            return -1;
        
        if (playerClients[oneCode].choice == CARD_TYPE.NONE)
            return twoCode;

        var pTwoLose = PerkSystem.GetWeakestType(playerClients[oneCode].choice);
        if (playerClients[twoCode].choice == pTwoLose
            || playerClients[twoCode].choice == CARD_TYPE.NONE)
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
                SendToClientResult(player, drawCode, GetOpponent(player));
            ClearChoices();
            ChangeGameState(SERVER_STATE.CHOICE_TIME);
        }
        else
        {
            DamagePlayer(result.loserClient, result.winnerClient);
            var playerDeath = GetPlayerDeath();
            if (playerDeath == null)
            {
                SendToClientResult(result.loserClient, loseCode, result.winnerClient);
                SendToClientResult(result.winnerClient, winCode, result.loserClient);
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
        foreach (var pClient in playerClients)
        {
            if (pClient.choice != CARD_TYPE.NONE)
                thisRoundInactive = false;
        }

        inactiveRounds = thisRoundInactive ? inactiveRounds + 1 : 0;
        if (inactiveRounds >= refs.globalConfig.maxInactiveRounds)
        {
            KickAllForInactivity();
            return true;
        }
        else
            return false;
    }

    private void SendToClientResult(PlayerClient playerClient, SIMPLE_RESULT result, PlayerClient opponentClient)
    {
        var stringContent = result + "/" + opponentClient.choice;
        var netMessage = new NetworkMessage
        {
            ClientID = playerClient.playerClientID,
            ObjectID = playerClient.playerObjectID,
            StringContent = stringContent,
            MessageType = (int)MESSAGE_TYPE.ROUND_RESULT,
            ValueOneContent = playerClient.playerHealth,
            ValueTwoContent = opponentClient.playerHealth
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

    private void DamagePlayer(PlayerClient playerClient, PlayerClient opponentClient)
        => playerClient.playerHealth -= GetComboDamage(opponentClient);

    private void ClearGameMatch()
    {
        round = 0;
        inactiveRounds = 0;
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
        for (var i = 0; i < playerClients.Count; i++)
            playerClients[i].choice = CARD_TYPE.EMPTY;
    }

    private bool AllPlayersChose()
    {
        var result = true;
        foreach (var playerClient in playerClients)
        {
            if (playerClient.choice == CARD_TYPE.EMPTY) 
                result = false;
        }

        return result;
    }

    /// <summary>Get damage based in combo</summary>
    /// <param name="pClient">PlayerClient with combo to be considered</param><returns></returns>
    private float GetComboDamage(PlayerClient pClient)
    {
        var result = refs.globalConfig.baseDamage;
        for (var i = 0; i < pClient.combo; i++)
            result *= refs.globalConfig.comboMultiplier;
        return result;
    }

    private void AddCombo(PlayerClient playerClient)
        => playerClient.combo++;
    
    private void ResetCombo(PlayerClient playerClient)
        => playerClient.combo = -1;

    private void ApplyCombos(PlayerClient winner, PlayerClient loser, bool isDraw = false)
    {
        if (isDraw)
        {
            ResetCombo(winner);
            ResetCombo(loser);
        }
        else
        {
            AddCombo(winner);
            ResetCombo(loser);
        }
    }

    #endregion
}