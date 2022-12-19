using System.Collections.Generic;
using FishNet.Broadcast;
using FishNet.Object;

public struct NetworkMessage : IBroadcast
{
    //client and obj id who send || if send from server, client and obj id to receive
    public int ClientID;
    public int ObjectID;
    public int MessageType;
    public string StringContent;
    public float ValueOneContent;
    public float ValueTwoContent;
}

public struct Match_Info
{
    public bool isDraw;
    public PlayerClient winnerClient;
    public PlayerClient loserClient;
    //public bool isInactivePlayers;
}