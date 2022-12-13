using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FishNet.Connection;
using FishNet.Object;
using FishNet;
using Tools;
using FishNet.Transporting;
using Unity.VisualScripting;

public class NetClientCommunicate : NetworkBehaviour
{
    private ScriptsReferences refs => ScriptsReferences.Instance;
    private int myID;

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
            myID = ClientManager.GetInstanceID();
        }
        
        InstanceFinder.ClientManager.RegisterBroadcast<NetworkMessage>(OnMessageReceived);
    }

    public void SendMessageToServer(NetworkMessage networkMessage) 
        => InstanceFinder.ClientManager.Broadcast(networkMessage);

    #endregion
    
    #region Private Methods

    private void OnDisable()
        => InstanceFinder.ClientManager.UnregisterBroadcast<NetworkMessage>(OnMessageReceived);

    private void OnMessageReceived(NetworkMessage message) => TreatMessage(message);

    private void TreatMessage(NetworkMessage message)
    {
        if (myID != message.ClientID || this.NetworkObject.ObjectId != message.ObjectID) return;
        Debug.Log("Received message of type " + (MESSAGE_TYPE)message.MessageType +
                  " - Content: [String] " + message.StringContent + " | [Value1] " + message.ValueOneContent +
                  " [Value2] " + message.ValueTwoContent + " - [ID's] Client: " + message.ClientID +
                  " | Object: " + message.ObjectID);

        switch (message.MessageType)
        {
            case (int)MESSAGE_TYPE.STRING:
                Debug.Log("Received string message: " + message.StringContent);
                if (message.StringContent.Contains("[SERVER]"))
                    LogMessage(message.StringContent);
                break;
            case (int)MESSAGE_TYPE.NEW_CONNECTION:
                refs.localManager.ConnectionSuccess(message.ValueOneContent);
                break;
            case (int)MESSAGE_TYPE.START_GAME:
                refs.localManager.GameInit();
                break;
            case (int)MESSAGE_TYPE.GAME_OVER:
                refs.localManager.GameOver(message.StringContent);
                break;
            case (int)MESSAGE_TYPE.ROUND_RESULT:
                refs.localManager.RunRoundResult(message.StringContent, message.ValueOneContent,
                    message.ValueTwoContent);
                break;
            case (int)MESSAGE_TYPE.CONNECTION_REFUSE:
                LogMessage(message.StringContent);
                break;
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

    private void LogMessage(string message)
    {
        if(refs.localManager.gameState is LOCAL_STATE.MENU)
            UIMenuManager.Instance.LogMessage(message);
        else
            ShowSimpleLogs.Instance.Log(message);
    }

    #endregion
}
