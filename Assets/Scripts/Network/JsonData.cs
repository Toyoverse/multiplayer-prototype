using System;
using UnityEngine;

public static class JsonData
{
    private static ScriptsReferences Refs => ScriptsReferences.Instance;

    public static NetworkMessage GetClientMessage(MESSAGE_TYPE _type, CARD_TYPE _choice = CARD_TYPE.EMPTY, int _choiceAmount = 0)
        => new()
        {
            ClientID = Refs.myNetClientCommunicate.ClientManager.GetInstanceID(),
            ObjectID = Refs.myNetClientCommunicate.NetworkObject.ObjectId,
            Content = JsonUtility.ToJson(new ClientNetMessage()
            {
                messageType = _type,
                version = Application.version,
                choice = _choice,
                choiceAmount = _choiceAmount
            })
        };

    public static ClientNetMessage GetClientMessage(string json) => JsonUtility.FromJson<ClientNetMessage>(json);

    public static NetworkMessage GetServerMessage(PlayerClient player, PlayerClient opponent, MESSAGE_TYPE type, 
        SIMPLE_RESULT _roundResult = SIMPLE_RESULT.NONE, string _textMessage = "")
        => new()
        {
            ClientID = player.playerClientID,
            ObjectID = player.playerObjectID,
            Content = JsonUtility.ToJson(new ServerNetMessage
            {
                messageType = type,
                textMessage = _textMessage,
                roundResult = _roundResult,
                playerHealth = player.playerHealth,
                playerCombo = player.combo,
                opponentHealth = opponent.playerHealth,
                opponentCombo = opponent.combo,
                opponentChoice = opponent.choice
            })
        };
    
    public static ServerNetMessage GetServerMessage(string json) => JsonUtility.FromJson<ServerNetMessage>(json);

    public static NetworkMessage GetServerSimpleStringMessage(int clientID, int objectID, string message)
        => new()
        {
            ClientID = clientID,
            ObjectID = objectID,
            Content = JsonUtility.ToJson(new ServerNetMessage
            {
                messageType = MESSAGE_TYPE.STRING,
                textMessage = message,
            })
        };
    
    public static NetworkMessage GetServerKickMessage(int clientID, int objectID, string kickInfo, KICK_REASON reason)
        => new()
        {
            ClientID = clientID,
            ObjectID = objectID,
            Content = JsonUtility.ToJson(new ServerNetMessage
            {
                messageType = MESSAGE_TYPE.CONNECTION_REFUSE,
                textMessage = kickInfo,
                kickReason = reason
            })
        };
}

[Serializable]
public class ServerNetMessage
{
    //Message
    public MESSAGE_TYPE messageType;
    public string textMessage;
    public KICK_REASON kickReason;
    //Player
    public SIMPLE_RESULT roundResult;
    public float playerHealth;
    public int playerCombo;
    //Opponent
    public float opponentHealth;
    public int opponentCombo;
    public CARD_TYPE opponentChoice;
}

[Serializable]
public class ClientNetMessage
{
    public MESSAGE_TYPE messageType;
    public string version;
    public CARD_TYPE choice;
    public int choiceAmount;
}
