using System;
using FishNet.Connection;

[Serializable]
public class PlayerClient
{
    public int playerClientID;
    public int playerObjectID;
    public float playerHealth;
    public NetworkConnection networkConnection;
    public CARD_TYPE choice;
    public int choiceAmount;
    public int combo;
}
