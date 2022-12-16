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

    private const string serverFullMessage =
        "[SERVER] The server is not accepting new connections, please try again later.";
    private const string versionWrongMessage = "Unable to connect to the server. The game version is outdated.";
    private const string inactiveMessage = "You have been logged out for inactivity.";

    #region Public Methods

    public void SendMessageToClient(NetworkMessage networkMessage) =>
        InstanceFinder.ServerManager.Broadcast(networkMessage);

    public void KickPlayer(PlayerClient client, KICK_REASON kickReason)
    {
        AutoKickMessage(client.playerClientID, client.playerObjectID, kickReason);
        StartCoroutine(TimeTools.InvokeInTime(client.networkConnection.Disconnect, 
            true, disconnectDelay));
    }

    #endregion
    
    #region Private Methods

    private void RemoveEvents()
    {
        InstanceFinder.ServerManager.UnregisterBroadcast<NetworkMessage>(OnMessageReceived);
        InstanceFinder.ServerManager.OnRemoteConnectionState -= OnRemoteConnectionStateListener;
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
                if (netMessage.StringContent == Application.version
                    && gameSystem.serverState is SERVER_STATE.WAIT_CONNECTIONS)
                    gameSystem.RegisterNewPlayerConnection(netMessage.ClientID, netMessage.ObjectID, conn);
                else
                {
                    var kickReason = gameSystem.serverState != SERVER_STATE.WAIT_CONNECTIONS
                        ? KICK_REASON.WRONG_VERSION
                        : KICK_REASON.SERVER_IS_FULL;
                    AutoKickMessage(netMessage.ClientID, netMessage.ObjectID, kickReason);
                    StartCoroutine(TimeTools.InvokeInTime(conn.Disconnect, true, disconnectDelay));
                }
                break;
        }
    }

    private void OnRemoteConnectionStateListener(NetworkConnection conn, RemoteConnectionStateArgs args /*ClientConnectionStateArgs args*/)
        => gameSystem.ClientDisconnected(conn, args);

    private void AutoKickMessage(int clientID, int objectID, KICK_REASON reason)
    {
        var message = reason switch
        {
            KICK_REASON.NONE => "Connection generic error.",
            KICK_REASON.SERVER_IS_FULL => serverFullMessage,
            KICK_REASON.WRONG_VERSION => versionWrongMessage,
            KICK_REASON.INACTIVE => inactiveMessage
        };
        var netMessage = new NetworkMessage()
        {
            ClientID = clientID,
            ObjectID = objectID,
            MessageType = (int)MESSAGE_TYPE.CONNECTION_REFUSE,
            StringContent = message,
            ValueOneContent = (int)reason
        };
        SendMessageToClient(netMessage);
    }

    #endregion
}
