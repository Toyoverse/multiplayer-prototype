using System.Collections.Generic;
using FishNet.Broadcast;
using FishNet.Object;

public struct NetworkMessage : IBroadcast
{
    //client and obj id who send || if send from server, client and obj id to receive
    public int ClientID;
    public int ObjectID;
    public int MessageType;
    public string Content;
    public float ValueContent;
}

public struct Match_Info
{
    public List<GameChoice> choices; //ObjectID, Choice
    public bool isDraw;
    public int winnerObjectID;
    public int loserObjectID;
    public int winnerClientID;
    public int loserClientID;
}