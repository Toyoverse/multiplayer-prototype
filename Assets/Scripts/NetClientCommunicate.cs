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
        switch (message.MessageType)
        {
            case (int)MESSAGE_TYPE.STRING:
                Debug.Log("Received string message: " + message.Content);
                break;
            case (int)MESSAGE_TYPE.NEW_CONNECTION:
                refs.localManager.ConnectionSuccess();
                break;
            case (int)MESSAGE_TYPE.START_GAME:
                refs.myStatusManager.InitHealth(message.ValueContent);
                refs.localManager.GameInit();
                break;
        }

        if (message.MessageType != (int)MESSAGE_TYPE.ROUND_RESULT)
            return;
        
        var myID = ClientManager.GetInstanceID();
        /*if (id == message.InstanceID || this.NetworkObject.ObjectId == message.NetworkObject.ObjectId)
            return;*/ //Esse if era pra ler a mensagem apenas quem não a tivesse enviado
       
        if (myID == message.ClientID && this.NetworkObject.ObjectId == message.ObjectID)
        {
            Debug.Log("Received message of type " + message.MessageType + ", received from " 
                      + message.ObjectID + ": " + message.Content);
            refs.localManager.RunRoundResult(message.Content, message.ValueContent);
        }
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
