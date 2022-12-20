using FishNet.Broadcast;

public struct NetworkMessage : IBroadcast
{
    //client and obj id who send || if send from server, client and obj id to receive
    public int ClientID;
    public int ObjectID;
    public string Content; 
    //Content is a JSON => Server send ServerNetMessage and read ClientNetMessage
    //Client send ClientNetMessage and read ServerNetMessage
}

public struct Match_Info
{
    public bool isDraw;
    public PlayerClient winnerClient;
    public PlayerClient loserClient;
}