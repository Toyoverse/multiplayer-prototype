using System;
using FishNet.Managing;
using FishNet.Transporting.Tugboat;
using Managers;
using Tools;
using UnityEngine;

public class ScriptsReferences : Singleton<ScriptsReferences>
{
    public PlayerInput playerInput;
    public GameObject uiMyHpTarget;
    public GameObject uiOpponentHpTarget;
    public LocalGameManager localManager;
    public NetClientCommunicate myNetClientCommunicate;
    public StatusManager myStatusManager;
    public NetworkHudCanvases myNetHudCanvas;
    public Tugboat myTugboat;
    public TimelineManager timelineManager;
    public GlobalConfigSO globalConfig;
    public DeckManager deckManager;
    public HandManager handManager;

    //c1ccd940e985bdb4bb6232d39d25a853
}
