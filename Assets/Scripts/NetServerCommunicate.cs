using System;
using Unity.VisualScripting;
using UnityEngine;
using FishNet;
using FishNet.Broadcast;
using FishNet.Connection;

public class NetServerCommunicate : MonoBehaviour
{
    [SerializeField] private GameSystem gameSystem;
    
    #region Private Methods
    
    private void OnEnable()
    {
        if (!InstanceFinder.IsServer)
        {
            gameObject.GetComponent<NetServerCommunicate>().enabled = false;
        }
        InstanceFinder.ServerManager.RegisterBroadcast<NetworkMessage>(OnMessageReceived);
        gameSystem ??= FindObjectOfType<GameSystem>();
    }
    
    private void OnDisable()
    {
        InstanceFinder.ServerManager.UnregisterBroadcast<NetworkMessage>(OnMessageReceived);
    }

    private void OnMessageReceived(NetworkConnection networkConnection, NetworkMessage message) 
        => TreatMessage(message);

    private void TreatMessage(NetworkMessage netMessage)
    {
        switch ((MESSAGE_TYPE)netMessage.MessageType)
        {
            case MESSAGE_TYPE.NONE:
                netMessage.Content = "Server receive message type is none.";
                netMessage.MessageType = (int)MESSAGE_TYPE.STRING;
                InstanceFinder.ServerManager.Broadcast(netMessage);
                break;
            case MESSAGE_TYPE.CARD_CHOICE:
                gameSystem.RegisterNewPlayerChoice(netMessage.ClientID, netMessage.ObjectID, netMessage.Content);
                break;
            case MESSAGE_TYPE.MATCH_RESULT:
                break;
        }
    }
    
    #endregion
    
    #region Public Methods

    public void SendToClientResult(int clientID, int objectID, string message)
    {
        var netMessage = new NetworkMessage
        {
            ClientID = clientID,
            ObjectID = objectID,
            Content = message,
            MessageType = (int)MESSAGE_TYPE.MATCH_RESULT
        };
        InstanceFinder.ServerManager.Broadcast(netMessage);
    }
    
    #endregion
}
