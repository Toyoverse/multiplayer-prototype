using System;
using Unity.VisualScripting;
using UnityEngine;
using FishNet;
using FishNet.Broadcast;
using FishNet.Connection;
using FishNet.Transporting;

public class NetServerCommunicate : MonoBehaviour
{
    [SerializeField] private GameSystem gameSystem;

    private int testClientCount = 0;
    
    #region Public Methods

    public void SendMessageToClient(NetworkMessage networkMessage) =>
        InstanceFinder.ServerManager.Broadcast(networkMessage);
    
    #endregion
    
    #region Private Methods

    private void Update()
    {
        if (InstanceFinder.ServerManager.Clients.Count != testClientCount)
        {
            testClientCount = InstanceFinder.ServerManager.Clients.Count;
            gameSystem.SendStringMessageForAllClients("ClientCount: " + testClientCount);
        }
    }

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
                gameSystem.RegisterPlayerChoice(netMessage.ClientID, netMessage.ObjectID, netMessage.Content);
                break;
            case MESSAGE_TYPE.NEW_CONNECTION:
                if (gameSystem.gameState != GAME_STATE.WAIT_CONNECTIONS)
                {
                    gameSystem.SendStringMessageForAllClients("[SERVER] The server is not accepting new connections, " +
                                                              "there is already a match in progress. Connected " +
                                                              "Clients: " + gameSystem.playersConnected);
                    return;
                }
                gameSystem.RegisterNewPlayerConnection(netMessage.ClientID, netMessage.ObjectID);
                break;
        }
    }

    private void OnRemoteConnectionStateListener(NetworkConnection conn, RemoteConnectionStateArgs args /*ClientConnectionStateArgs args*/)
    {
        gameSystem.ClientDisconnected(conn, args);
        /*if (args.ConnectionState == RemoteConnectionState.Stopped)
        { 
            gameSystem.ClientDisconnected(conn.ClientId);
        }*/
    }

    #endregion
}
