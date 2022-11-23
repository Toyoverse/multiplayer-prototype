using Tools;
using UnityEngine;

public class ScriptsReferences : Singleton<ScriptsReferences>
{
    public PlayerInput playerInput;
    public GameObject uiCanvasObject;
    public LocalGameManager localManager;
    public NetClientCommunicate myNetClientCommunicate;
    public StatusManager myStatusManager;
}
