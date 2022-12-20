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
        var content = JsonData.GetServerMessage(message.Content);
        Debug.Log("Received message: " + message.Content);

        switch (content.messageType)
        {
            case MESSAGE_TYPE.STRING:
                LogMessage(content.textMessage);
                break;
            case MESSAGE_TYPE.NEW_CONNECTION:
                refs.localManager.ConnectionSuccess(content.playerHealth);
                break;
            case MESSAGE_TYPE.START_GAME:
                refs.localManager.GameInit();
                break;
            case MESSAGE_TYPE.GAME_OVER:
                refs.localManager.GameOver(content.roundResult);
                break;
            case MESSAGE_TYPE.ROUND_RESULT:
                refs.localManager.RunRoundResult(content);
                break;
            case MESSAGE_TYPE.CONNECTION_REFUSE:
                ClientKickedThreat(content.textMessage, content.kickReason);
                break;
        }
    }
    
    private void SendNewConnectionToServer()
    {
        var netMessage = JsonData.GetClientMessage(MESSAGE_TYPE.NEW_CONNECTION);
        SendMessageToServer(netMessage);
    }

    private void LogMessage(string message)
    {
        if(refs.localManager.gameState is LOCAL_STATE.MENU)
            UIMenuManager.Instance.LogMessage(message);
        else
            ShowSimpleLogs.Instance.Log(message);
    }

    private void ClientKickedThreat(string message, KICK_REASON reason)
    {
        if(refs.localManager.gameState != LOCAL_STATE.MENU)
            refs.localManager.BackToMenu();
        
        UIMenuManager.Instance.LogMessage(message + "\n[REASON: " + reason + "]");
    }

    #endregion
}
