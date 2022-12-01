using System;
using System.Collections.Generic;
using System.Linq;
using FishNet;
using UnityEngine;
using FishNet.Connection;
using FishNet.Transporting;

public class GameSystem : MonoBehaviour
{
    [Header("REFERENCES")]
    [SerializeField] private NetServerCommunicate netServer;

    [Header("GAME STATUS")]
    [SerializeField] private List<GameChoice> playersChoices;
    [SerializeField] private List<PlayerHealth> playerHealths;
    [SerializeField] private int round = 0;
    [SerializeField] private int inactiveRounds = 0;
    public int playersConnected => playerHealths?.Count ?? 0;

    [Header("LIFE VALUES")] 
    [SerializeField] private float maxHealth = 3;
    [SerializeField] private float roundDamage = 1;

    private const string winCode = "WIN";
    private const string loseCode = "LOSE";
    private const string drawCode = "DRAW";

    private float timer;
    private int roundTimeLimit = 12;
    private int inactiveRoundsLimit = 3;

    public GAME_STATE gameState;

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
    
    public void RegisterPlayerChoice(int clientID, int objectID, string pChoice)
    {
        /*if (playersChoices == null)
            ResetPlayerChoices();*/

        var _pChoice = GetTypeFromString(pChoice);

        foreach (var gameChoice in playersChoices)
        {
            if (gameChoice.playerObjectID == objectID && gameChoice.playerClientID == clientID)
            {
                gameChoice.choice = _pChoice;
                return;
            }
        }
        
        /*var item = new GameChoice
        {
            playerClientID = clientID,
            playerObjectID = objectID,
            choice = _pChoice
        };
        playersChoices.Add(item);*/

        //CheckChoices();
        if(AllPlayersChose())
            GoToCompareTime();
    }

    public void ClientDisconnected(/*ClientConnectionStateArgs args*/ NetworkConnection conn, RemoteConnectionStateArgs args)
    {
        /*var playerDisconnected = GetPlayerHealthByClientID(clientID);
        if (playerDisconnected == null)
            return;
        playerDisconnected.playerHealth = 0;*/
        if (args.ConnectionState is RemoteConnectionState.Started)
            return;
        GameOverAllWin();
        var message = "ClientDisconnected_Args: args.connectionID: " + args.ConnectionId + ", args.connectionState: " 
                      + args.ConnectionState + "args.transportIndex: " + args.TransportIndex + 
                      " | NetConn.ClientID: " + conn.ClientId;
        /*var message = "ClientDisconnected_Args: " + "connectionState: " + args.ConnectionState
                      + ", transportIndex: " + args.TransportIndex;*/
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
        const int one = 0;
        const int two = 1;

        var matchResult = new Match_Info
        {
            choices = playersChoices,
            isDraw = playersChoices[one].choice == playersChoices[two].choice ? true : false,
            winnerObjectID = -1,
            loserObjectID = -1,
            loserClientID = -1,
            winnerClientID = -1,
            isInactivePlayers = playersChoices[one].choice == CARD_TYPE.NONE && playersChoices[two].choice == CARD_TYPE.NONE
        };

        if (matchResult.isDraw)
            return matchResult;

        switch (playersChoices[one].choice)
        {
            case CARD_TYPE.ROCK:
                switch (playersChoices[two].choice)
                {
                    case CARD_TYPE.PAPER:
                        SetPlayerTwoWinner(ref matchResult.winnerObjectID, ref matchResult.loserObjectID,
                            playersChoices[one].playerObjectID, playersChoices[two].playerObjectID);
                        break;
                    case CARD_TYPE.SCISSOR:
                        SetPlayerOneWinner(ref matchResult.winnerObjectID, ref matchResult.loserObjectID,
                            playersChoices[one].playerObjectID, playersChoices[two].playerObjectID);
                        break;
                    case CARD_TYPE.NONE:
                        SetPlayerOneWinner(ref matchResult.winnerObjectID, ref matchResult.loserObjectID,
                            playersChoices[one].playerObjectID, playersChoices[two].playerObjectID);
                        break;
                }
                break;
            case CARD_TYPE.PAPER:
                switch (playersChoices[two].choice)
                {
                    case CARD_TYPE.ROCK:
                        SetPlayerOneWinner(ref matchResult.winnerObjectID, ref matchResult.loserObjectID,
                            playersChoices[one].playerObjectID, playersChoices[two].playerObjectID);
                        break;
                    case CARD_TYPE.SCISSOR:
                        SetPlayerTwoWinner(ref matchResult.winnerObjectID, ref matchResult.loserObjectID,
                            playersChoices[one].playerObjectID, playersChoices[two].playerObjectID);
                        break;
                    case CARD_TYPE.NONE:
                        SetPlayerOneWinner(ref matchResult.winnerObjectID, ref matchResult.loserObjectID,
                            playersChoices[one].playerObjectID, playersChoices[two].playerObjectID);
                        break;
                }
                break;
            case CARD_TYPE.SCISSOR:
                switch (playersChoices[two].choice)
                {
                    case CARD_TYPE.ROCK:
                        SetPlayerTwoWinner(ref matchResult.winnerObjectID, ref matchResult.loserObjectID,
                            playersChoices[one].playerObjectID, playersChoices[two].playerObjectID);
                        break;
                    case CARD_TYPE.PAPER:
                        SetPlayerOneWinner(ref matchResult.winnerObjectID, ref matchResult.loserObjectID,
                            playersChoices[one].playerObjectID, playersChoices[two].playerObjectID);
                        break;
                    case CARD_TYPE.NONE:
                        SetPlayerOneWinner(ref matchResult.winnerObjectID, ref matchResult.loserObjectID,
                            playersChoices[one].playerObjectID, playersChoices[two].playerObjectID);
                        break;
                }
                break;
            case CARD_TYPE.NONE:
                if (playersChoices[two].choice != CARD_TYPE.NONE)
                    SetPlayerTwoWinner(ref matchResult.winnerObjectID, ref matchResult.loserObjectID,
                        playersChoices[one].playerObjectID, playersChoices[two].playerObjectID);
                break;
        }

        matchResult.winnerClientID = GetPlayerClientID(matchResult.winnerObjectID);
        matchResult.loserClientID = GetPlayerClientID(matchResult.loserObjectID);
        return matchResult;
    }

    private void SetPlayerOneWinner(ref int winID, ref int loseID, int oneID, int twoID)
    {
        winID = oneID;
        loseID = twoID;
    }
    
    private void SetPlayerTwoWinner(ref int winID, ref int loseID, int oneID, int twoID)
    {
        winID = twoID;
        loseID = oneID;
    }

    private int GetPlayerClientID(int objectID)
    {
        var result = -1;
        for (var i = 0; i < playersChoices.Count; i++)
        {
            if (objectID == playersChoices[i].playerObjectID)
                result = playersChoices[i].playerClientID;
        }

        return result;
    }

    private CARD_TYPE GetTypeFromString(string type)
    {
        return type.ToUpper() switch
        {
            "NONE" => CARD_TYPE.NONE,
            "ROCK" => CARD_TYPE.ROCK,
            "PAPER" => CARD_TYPE.PAPER,
            "SCISSOR" => CARD_TYPE.SCISSOR,
            _ => throw new ArgumentOutOfRangeException()
        };
    }

    private void CheckChoices()
    {
        //SendGameSystemInfoToClients();
        
        var result = GetMatchResult();
        if (result.isDraw)
        {
            if (result.isInactivePlayers)
            {
                inactiveRounds++;
                if (inactiveRounds >= inactiveRoundsLimit)
                {
                    GameOverAllLose();
                    return;
                }
            }

            foreach (var gameChoice in playersChoices)
                SendToClientResult(gameChoice, drawCode);
        }
        else
        {
            round++;
            DamagePlayer(result.loserObjectID);
            var playerDeath = GetPlayerDeath();
            if (playerDeath == null)
            {
                SendToClientResult(GetPlayerChoice(result.loserObjectID), loseCode);
                SendToClientResult(GetPlayerChoice(result.winnerObjectID), winCode);
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
            ValueContent = GetPlayerHealth(playerChoice.playerObjectID)
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

    private void DamagePlayer(int objectID)
    {
        for (var i = 0; i < playerHealths.Count; i++)
        {
            if (playerHealths[i].playerObjectID == objectID)
                playerHealths[i].playerHealth -= roundDamage;
        }
    }

    private float GetPlayerHealth(int objectID)
    {
        return (from t in playerHealths where t.playerObjectID == objectID select t.playerHealth).FirstOrDefault();
    }

    private void ClearGameMatch()
    {
        playerHealths = new List<PlayerHealth>();
        round = 0;
        inactiveRounds = 0;
        timer = 0;
        playersChoices?.Clear();
    }

    private GameChoice GetPlayerChoice(int objectID)
    {
        return playersChoices.FirstOrDefault(gameChoice => objectID == gameChoice.playerObjectID);
    }

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
                "\nPlayer[" + playerHealths[0].playerObjectID + "] Health: " + playerHealths[0].playerHealth +
                "\nPlayer[" + playerHealths[1].playerObjectID + "] Health: " + playerHealths[1].playerHealth;
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

    private void Update()
    {
        if (gameState != GAME_STATE.CHOICE_TIME)
            return;
        timer += Time.deltaTime;
        if (timer >= roundTimeLimit)
        {
            GoToCompareTime();
        }
    }

    private void GoToCompareTime()
    {
        ChangeGameState(GAME_STATE.COMPARE_TIME);
        timer = 0;
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