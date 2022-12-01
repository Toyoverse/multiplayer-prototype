using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FishNet.Connection;
using FishNet.Object;
using FishNet;
using Tools;

public class NetClientCommunicate : NetworkBehaviour
{
    private ScriptsReferences refs => ScriptsReferences.Instance;

    #region Public Methods
    
    public override void OnStartClient()
    {
        base.OnStartClient();

        if (!base.IsOwner)
        {
            gameObject.GetComponent<NetClientCommunicate>().enabled = false;
            //return;
        }
        else
        {
            UIMenuManager.Instance.ConnectedSuccess();
            refs.myNetClientCommunicate = this;
            SendNewConnectionToServer();
        }
        
        InstanceFinder.ClientManager.RegisterBroadcast<NetworkMessage>(OnMessageReceived);
    }

    public void SendMessageToServer(NetworkMessage networkMessage) => InstanceFinder.ClientManager.Broadcast(networkMessage);

    #endregion
    
    #region Private Methods

    private void OnDisable()
        => InstanceFinder.ClientManager.UnregisterBroadcast<NetworkMessage>(OnMessageReceived);

    private void OnMessageReceived(NetworkMessage message) => TreatMessage(message);

    private void TreatMessage(NetworkMessage message)
    {
        var myID = ClientManager.GetInstanceID();
        if (!(myID == message.ClientID && this.NetworkObject.ObjectId == message.ObjectID))
            return;
        
        Debug.Log("Received message of type " + (MESSAGE_TYPE)message.MessageType + 
                  " - Content: [String] " + message.Content + " | [Value] " + message.ValueContent);
        
        switch (message.MessageType)
        {
            case (int)MESSAGE_TYPE.STRING:
                Debug.Log("Received string message: " + message.Content);
                break;
            case (int)MESSAGE_TYPE.NEW_CONNECTION:
                refs.localManager.ConnectionSuccess(message.ValueContent);
                break;
            case (int)MESSAGE_TYPE.START_GAME:
                refs.localManager.GameInit();
                break;
            case (int)MESSAGE_TYPE.GAME_OVER:
                refs.localManager.GameOver(message.Content);
                break;
        }

        if (message.MessageType != (int)MESSAGE_TYPE.ROUND_RESULT)
            return;
        
        refs.localManager.RunRoundResult(message.Content, message.ValueContent);
    }
    
    private void SendNewConnectionToServer()
    {
        var netMessage = new NetworkMessage()
        {
            ClientID = ClientManager.GetInstanceID(),
            ObjectID = this.NetworkObject.ObjectId,
            MessageType = (int)MESSAGE_TYPE.NEW_CONNECTION
        };
        SendMessageToServer(netMessage);
    }

    #endregion
}
