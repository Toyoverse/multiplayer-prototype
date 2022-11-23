using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using FishNet;
using UnityEngine;
using FishNet.Connection;
using FishNet.Object;
using Debug = UnityEngine.Debug;

public class GameSystem : NetworkBehaviour
{
    [Header("REFERENCES")]
    [SerializeField] private NetServerCommunicate netServer;

    [Header("GAME STATUS")]
    [SerializeField] private List<GameChoice> playersChoices;
    [SerializeField] private List<PlayerHealth> playerHealths;
    [SerializeField] private int round;
    public int playersConnected => playerHealths?.Count ?? 0;

    [Header("LIFE VALUES")] 
    [SerializeField] private float maxHealth = 3;
    [SerializeField] private float roundDamage = 1;

    private const string winCode = "WIN";
    private const string loseCode = "LOSE";
    private const string drawCode = "DRAW";

    #region Events

    private delegate void EndRound();
    private EndRound onEndRound;

    #endregion

    #region Public methods
    
    public void RegisterNewPlayerConnection(int clientID, int objectID)
    {
        if (playerHealths == null)
            ResetPlayersHealth();

        if (playerHealths.Count == 2)
            return;
        
        var newPlayer = new PlayerHealth()
        {
            playerClientID = clientID,
            playerObjectID = objectID,
            playerHealth = maxHealth
        };
        playerHealths.Add(newPlayer);

        SendToClientHealthInit(newPlayer);

        if (playersConnected >= 2)
            SendStartGameForAllClients();
    }
    
    public void RegisterNewPlayerChoice(int clientID, int objectID, string pChoice)
    {
        if (playersChoices == null)
            ResetPlayerChoices();

        var _pChoice = GetTypeFromString(pChoice);

        /*foreach (var gameChoice in playersChoices)
        {
            if (gameChoice.playerObjectID == objectID)
            {
                gameChoice.choice = _pChoice;
                return;
            }
        }*/
        
        var item = new GameChoice
        {
            playerClientID = clientID,
            playerObjectID = objectID,
            choice = _pChoice
        };
        playersChoices.Add(item);

        CheckChoices();
    }

    #endregion
    
    #region Private Methods

    private void Start()
    {
        if (!InstanceFinder.IsServer)
            this.gameObject.GetComponent<GameSystem>().enabled = false;
        netServer ??= FindObjectOfType<NetServerCommunicate>();
        onEndRound += ResetPlayerChoices;
        onEndRound += CheckGameOver;
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
            winnerClientID = -1
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
        if (playersChoices.Count != 2)
            return;

        var result = GetMatchResult();
        if (result.isDraw)
        {
            foreach (var gameChoice in playersChoices)
                SendToClientResult(gameChoice, drawCode);
        }
        else
        {
            DamagePlayer(result.loserObjectID);
            SendToClientResult(GetPlayerChoice(result.loserObjectID), loseCode);
            SendToClientResult(GetPlayerChoice(result.winnerObjectID), winCode);
        }
        onEndRound?.Invoke();
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

    private void ResetPlayerChoices()
    {
        playersChoices = new List<GameChoice>();
        round++;
    }
    
    private void CheckGameOver()
    {
        var playerDeath = GetPlayerDeath();
        if (playerDeath != null)
        {
            round = 0;
            ResetPlayersHealth();
            //TODO: DISCONNECT CLIENTS
        }
    }

    private PlayerHealth GetPlayerDeath()
    {
        return playerHealths.FirstOrDefault(player => player.playerHealth <= 0.1f);
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

    private void ResetPlayersHealth()
    {
        playerHealths = new List<PlayerHealth>();
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