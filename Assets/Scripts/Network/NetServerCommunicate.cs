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
    private ScriptsReferences Refs => ScriptsReferences.Instance;

    #region Public Methods

    public void SendMessageToClient(NetworkMessage networkMessage) =>
        InstanceFinder.ServerManager.Broadcast(networkMessage);

    public void KickPlayer(PlayerClient client, KICK_REASON kickReason)
    {
        AutoKickMessage(client.playerClientID, client.playerObjectID, kickReason);
        StartCoroutine(TimeTools.InvokeInTime(client.networkConnection.Disconnect, 
            true, Refs.globalConfig.serverKickDelay));
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
        var content = JsonData.GetClientMessage(netMessage.Content);
        switch (content.messageType)
        {
            case MESSAGE_TYPE.NONE:
                var returnMessage = JsonData.GetServerSimpleStringMessage(netMessage.ClientID, netMessage.ObjectID,
                    "Server receive message type is none.");
                InstanceFinder.ServerManager.Broadcast(returnMessage);
                break;
            case MESSAGE_TYPE.CARD_CHOICE:
                gameSystem.RegisterPlayerChoice(netMessage.ClientID, netMessage.ObjectID, content.choice,
                    content.choiceAmount);
                break;
            case MESSAGE_TYPE.NEW_CONNECTION:
                if (content.version == Application.version
                    && gameSystem.serverState is SERVER_STATE.WAIT_CONNECTIONS)
                    gameSystem.RegisterNewPlayerConnection(netMessage.ClientID, netMessage.ObjectID, conn);
                else
                {
                    var kickReason = gameSystem.serverState != SERVER_STATE.WAIT_CONNECTIONS
                        ? KICK_REASON.WRONG_VERSION
                        : KICK_REASON.SERVER_IS_FULL;
                    AutoKickMessage(netMessage.ClientID, netMessage.ObjectID, kickReason);
                    StartCoroutine(TimeTools.InvokeInTime(conn.Disconnect, true, 
                        Refs.globalConfig.serverKickDelay));
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
            KICK_REASON.SERVER_IS_FULL => Refs.globalConfig.serverIsFullMessage,
            KICK_REASON.WRONG_VERSION => Refs.globalConfig.versionWrongMessage,
            KICK_REASON.INACTIVE => Refs.globalConfig.inactiveMessage,
            _ => Refs.globalConfig.connectionGenericErrorMessage
        };
        var netMessage = JsonData.GetServerKickMessage(clientID, objectID, message, reason);
        SendMessageToClient(netMessage);
    }

    #endregion
}
