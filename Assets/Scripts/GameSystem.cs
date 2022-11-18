using System;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;
using FishNet.Connection;
using FishNet.Object;

public class GameSystem : NetworkBehaviour
{
    [Header("CHOICES")] 
    [SerializeField] private Dictionary<int, SKILL_STATE> playerStates = new Dictionary<int, SKILL_STATE>(); //id, state
    [SerializeField] private int oneID; //player one
    [SerializeField] private int twoID; //player two

    #region Private Methods
    
    private void CompareSkills()
    {
        if(playerStates[oneID] is SKILL_STATE.NONE || playerStates[twoID] is SKILL_STATE.NONE)
            return;
        SendPlayerResult(oneID, GeneralCompare(oneID, twoID));
        SendPlayerResult(twoID, GeneralCompare(twoID, oneID));
    }

    private MATCH_RESULT GeneralCompare(int playerID, int opponentID)
    {
        var result = MATCH_RESULT.NONE;
        if (playerStates[playerID] == playerStates[opponentID])
            return MATCH_RESULT.DRAW;
        switch (playerStates[playerID])
        {
            case SKILL_STATE.ROCK:
                result = playerStates[opponentID] switch
                {
                    SKILL_STATE.ROCK => MATCH_RESULT.DRAW,
                    SKILL_STATE.PAPER => MATCH_RESULT.LOSE,
                    SKILL_STATE.SCISSOR => MATCH_RESULT.WIN,
                    _ => MATCH_RESULT.NONE
                };
                break;
            case SKILL_STATE.PAPER:
                result = playerStates[opponentID] switch
                {
                    SKILL_STATE.ROCK => MATCH_RESULT.WIN,
                    SKILL_STATE.PAPER => MATCH_RESULT.DRAW,
                    SKILL_STATE.SCISSOR => MATCH_RESULT.LOSE,
                    _ => MATCH_RESULT.NONE
                };
                break;
            case SKILL_STATE.SCISSOR:
                result = playerStates[opponentID] switch
                {
                    SKILL_STATE.ROCK => MATCH_RESULT.LOSE,
                    SKILL_STATE.PAPER => MATCH_RESULT.WIN,
                    SKILL_STATE.SCISSOR => MATCH_RESULT.DRAW,
                    _ => MATCH_RESULT.NONE
                };
                break;
            default:
                result = MATCH_RESULT.NONE;
                break;
        }
        
        return result;
    }
    
    #endregion
    
    #region Network Methods

    public override void OnStartClient()
    {
        base.OnStartClient();
        if (!IsServer)
        {
            gameObject.GetComponent<GameSystem>().enabled = false;
        }
    }

    [ServerRpc]
    public void ConfirmSkillServer(int playerID, SKILL_STATE state) => ConfirmSkill(playerID, state);

    public void ConfirmSkill(int playerID, SKILL_STATE state)
    {
        playerStates[playerID] = state;
    }

    public void SendPlayerResult(int playerID, MATCH_RESULT result)
    {
        
    }

    public void ConnectNewPlayer(int playerID)
    {
        playerStates.Add(playerID, SKILL_STATE.NONE);
    }
    
    #endregion
}

public enum SKILL_STATE
{
    NONE = 0,
    ROCK = 1,
    PAPER = 2,
    SCISSOR = 3
}

public enum MATCH_RESULT
{
    NONE = 0,
    LOSE = 1,
    DRAW = 2,
    WIN = 3
}
