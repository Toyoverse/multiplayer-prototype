using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FishNet.Connection;
using FishNet.Object;
using FishNet;

public class NetClientCommunicate : NetworkBehaviour
{
    [Header("REFERENCES")] 
    private ScriptsReferences refs;

    #region Public Methods
    
    public override void OnStartClient()
    {
        base.OnStartClient();
        if (refs == null)
            refs = FindObjectOfType<ScriptsReferences>();
        if (!base.IsOwner)
        {
            gameObject.GetComponent<NetClientCommunicate>().enabled = false;
            //return;
        }
        else
        {
            refs.playerInput.PlayerInputsInit(this);
        }
        
        InstanceFinder.ClientManager.RegisterBroadcast<NetworkMessage>(OnMessageReceived);
    }

    public void SendChoiceToServer(string choice)
    {
        var netMessage = new NetworkMessage()
        {
            ClientID = ClientManager.GetInstanceID(),
            ObjectID = this.NetworkObject.ObjectId,
            MessageType = (int)MESSAGE_TYPE.CARD_CHOICE,
            Content = choice
        };
        InstanceFinder.ClientManager.Broadcast(netMessage);
    }

    #endregion
    
    #region Private Methods

    private void OnDisable()
        => InstanceFinder.ClientManager.UnregisterBroadcast<NetworkMessage>(OnMessageReceived);

    private void OnMessageReceived(NetworkMessage message) => TreatMessage(message);

    private void TreatMessage(NetworkMessage message)
    {
        if (message.MessageType == (int)MESSAGE_TYPE.STRING)
            Debug.Log("Received string message: " + message.Content);

        if (message.MessageType != (int)MESSAGE_TYPE.MATCH_RESULT)
            return;
        
        var myID = ClientManager.GetInstanceID();
        /*if (id == message.InstanceID || this.NetworkObject.ObjectId == message.NetworkObject.ObjectId)
            return;*/ //Esse if era pra ler a mensagem apenas quem não a tivesse enviado
       
        if (myID == message.ClientID && this.NetworkObject.ObjectId == message.ObjectID)
        {
            Debug.Log("Received message of type " + message.MessageType + ", received from " 
                      + message.ObjectID + ": " + message.Content);
            RunResult(message.Content);
        }
    }

    private void RunResult(string result)
    {
        switch (result.ToUpper())
        {
            case "WIN":
                ShowLogs.Instance.Log("ROUND WIN!\nStarting a new round, please wait...");
                break;
            case "DRAW":
                ShowLogs.Instance.Log("DRAW!\nStarting a new round, please wait...");
                break;
            case "LOSE":
                ShowLogs.Instance.Log("ROUND LOSE!\nStarting a new round, please wait...");
                break;
        }

        refs.playerInput.RoundInit();
    }

    #endregion
}
