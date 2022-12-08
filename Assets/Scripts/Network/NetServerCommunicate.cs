using System;
using Unity.VisualScripting;
using UnityEngine;
using FishNet;
using FishNet.Broadcast;
using FishNet.Connection;
using FishNet.Transporting;
using Tools;

public class NetServerCommunicate : MonoBehaviour
{
    [SerializeField] private GameSystem gameSystem;
    public float disconnectDelay = 2;

    #region Public Methods

    public void SendMessageToClient(NetworkMessage networkMessage) =>
        InstanceFinder.ServerManager.Broadcast(networkMessage);
    
    public void RemoveEvents()
    {
        InstanceFinder.ServerManager.UnregisterBroadcast<NetworkMessage>(OnMessageReceived);
        InstanceFinder.ServerManager.OnRemoteConnectionState -= OnRemoteConnectionStateListener;
    }
    
    #endregion
    
    #region Private Methods

    private void OnEnable()
    {
        if (!InstanceFinder.IsServer)
        {
            gameObject.GetComponent<NetServerCommunicate>().enabled = false;
        }
        InstanceFinder.ServerManager.RegisterBroadcast<NetworkMessage>(OnMessageReceived);
        gameSystem ??= FindObjectOfType<GameSystem>();
        InstanceFinder.ServerManager.OnRemoteConnectionState += OnRemoteConnectionStateListener;
        //InstanceFinder.ServerManager.NetworkManager.TransportManager.Transport.OnClientConnectionState += OnRemoteConnectionStateListener;
    }
    
    private void OnDisable() => RemoveEvents();

    private void OnMessageReceived(NetworkConnection networkConnection, NetworkMessage message) 
        => TreatMessage(message, networkConnection);

    private void TreatMessage(NetworkMessage netMessage, NetworkConnection conn)
    {
        switch ((MESSAGE_TYPE)netMessage.MessageType)
        {
            case MESSAGE_TYPE.NONE:
                netMessage.StringContent = "Server receive message type is none.";
                netMessage.MessageType = (int)MESSAGE_TYPE.STRING;
                InstanceFinder.ServerManager.Broadcast(netMessage);
                break;
            case MESSAGE_TYPE.CARD_CHOICE:
                gameSystem.RegisterPlayerChoice(netMessage.ClientID, netMessage.ObjectID, int.Parse(netMessage.StringContent));
                break;
            case MESSAGE_TYPE.NEW_CONNECTION:
                if (gameSystem.serverState != SERVER_STATE.WAIT_CONNECTIONS)
                {
                    AutoKickMessage(netMessage.ClientID, netMessage.ObjectID);
                    StartCoroutine(TimeTools.InvokeInTime(conn.Disconnect, true, disconnectDelay));
                }
                else
                    gameSystem.RegisterNewPlayerConnection(netMessage.ClientID, netMessage.ObjectID, conn);
                break;
        }
    }

    private void OnRemoteConnectionStateListener(NetworkConnection conn, RemoteConnectionStateArgs args /*ClientConnectionStateArgs args*/)
        => gameSystem.ClientDisconnected(conn, args);

    private void AutoKickMessage(int clientID, int objectID)
    {
        var netMessage = new NetworkMessage()
        {
            ClientID = clientID,
            ObjectID = objectID,
            MessageType = (int)MESSAGE_TYPE.STRING,
            StringContent = "[SERVER] The server is not accepting new connections, please try again later.",
            ValueContent = -1
        };
        SendMessageToClient(netMessage);
    }

    #endregion
}
