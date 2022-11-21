using System;
using System.Collections.Generic;
using System.Diagnostics;
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
    [SerializeField] private int round;

    #region Private Methods

    private void Start()
    {
        if (!InstanceFinder.IsServer)
            this.gameObject.GetComponent<GameSystem>().enabled = false;
        netServer ??= FindObjectOfType<NetServerCommunicate>();
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

    private CARD_TYPE GetPlayerChoice(int objectID)
    {
        var result = CARD_TYPE.NONE;
        for (var i = 0; i < playersChoices.Count; i++)
        {
            if (objectID == playersChoices[i].playerObjectID)
                result = playersChoices[i].choice;
        }

        return result;
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
            "SCISSOR" => CARD_TYPE.SCISSOR
        };
    }

    private void CheckChoices()
    {
        if (playersChoices.Count < 2)
            return;
        var result = GetMatchResult();
        if (result.isDraw)
        {
            netServer.SendToClientResult(playersChoices[0].playerClientID, playersChoices[0].playerObjectID, "DRAW");
            netServer.SendToClientResult(playersChoices[1].playerClientID, playersChoices[1].playerObjectID, "DRAW");
            ResetPlayerChoices();
            return;
        }
        netServer.SendToClientResult(result.loserClientID, result.loserObjectID, "LOSE");
        netServer.SendToClientResult(result.winnerClientID, result.winnerObjectID, "WIN");
        ResetPlayerChoices();
    }

    private void ResetPlayerChoices()
    {
        playersChoices = new List<GameChoice>();
        round++;
    }

    #endregion
    
    #region Public methods
    
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
}

[Serializable]
public class GameChoice
{
    public int playerClientID;
    public int playerObjectID;
    public CARD_TYPE choice;
}