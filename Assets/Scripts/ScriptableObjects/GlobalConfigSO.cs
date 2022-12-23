using UnityEditor;
using UnityEngine;

[CreateAssetMenu(fileName = "GlobalConfigSO", menuName = "ScriptableObjects/GlobalConfigSO", order = 0)]
public class GlobalConfigSO : ScriptableObject
{
    [Header("CHEAT")] 
    public string lanCheat = "toyolanmode";
    [Header("HEALTH")]
    public float maxHealth = 100;
    [Header("DAMAGE & COMBO")]
    public float baseDamage = 10;
    public float comboMultiplier = 1.5f;
    public float repeatCardMultiplier = 1.25f;
    public float drawDamageMultiplier = 0.25f;
    [Header("ROUNDS")] 
    public int maxInactiveRounds = 5;
    public float secondsPerRound = 10;
    [Header("CARDS")]
    public int maxDeckAmount = 30;
    public int maxCardsPerType = 10;
    public int cardsDrawnPerRound = 3;
    public int minCardsPerTypeInHand = 1;
    [Header("NETWORK")] 
    public float serverKickDelay = 2;
    public float endGameDisconnectDelay = 5;
    [Header("NETWORK MESSAGES")]
    [TextArea] public string successConnectionMessage = "Connection success!\nWaiting for an opponent...";
    [TextArea] public string serverIsFullMessage = 
        "The server is not accepting new connections, please try again later.";
    [TextArea] public string versionWrongMessage = "Unable to connect to the server. The game version is outdated.";
    [TextArea] public string inactiveMessage = "You have been logged out for inactivity.";
    [TextArea] public string disconnectedMessage = "You have been disconnected from the server.";
    [TextArea] public string disconnectCountMessage = "You will be disconnected from the server in ";
    [TextArea] public string connectionGenericErrorMessage = "Connection generic error.";
    [Header("ROUNDS & MATCH MESSAGES")]
    [TextArea] public string roundInitMessage = "Round started!\nMake your move!";
    [TextArea] public string choiceMessage = "You chose ";
    [TextArea] public string opponentWaitMessage = "\nWaiting for the opponent's move...";
    [TextArea] public string winMessage = "ROUND WIN!";
    [TextArea] public string loseMessage = "ROUND LOSE!";
    [TextArea] public string drawMessage = "DRAW!";
    [TextArea] public string startingRoundMessage = "Starting a new round...";
    [TextArea] public string waitMessage = "Please wait.";
    [TextArea] public string matchWinMessage = "You WON the game!\nCongratulations!";
    [TextArea] public string matchLoseMessage = "You lost the game. :(\nDon't get discouraged, try again!";
    [TextArea] public string opponentCardMessage = "Your opponent chose ";
    [TextArea] public string backToMenuCountMessage = "Back to menu in ";
    [TextArea] public string genericErrorMessage = "Something went wrong, please try restarting the game.";
    [TextArea] public string roundTimeEndMessage = "TIME OUT!";

    [Space(30)]
    [Header("CAUTION! THIS BUTTON RESETS ALL VALUES!")] 
    public bool iReallyWantToResetEverything = false;

    public void ResetValues()
    {
        if (!iReallyWantToResetEverything)
        {
            Debug.LogError("Values have not been reset. To reset all values you need to check the option " +
                           "'I really want to reset everything'.");
            return;
        }
        lanCheat = "toyolanmode";
        maxHealth = 100;
        baseDamage = 10;
        comboMultiplier = 1.5f;
        repeatCardMultiplier = 1.25f;
        drawDamageMultiplier = 0.25f;
        maxInactiveRounds = 5;
        secondsPerRound = 10;
        maxDeckAmount = 30;
        maxCardsPerType = 10;
        cardsDrawnPerRound = 3;
        minCardsPerTypeInHand = 1;
        serverKickDelay = 2;
        endGameDisconnectDelay = 5;
        successConnectionMessage = "Connection success!\nWaiting for an opponent...";
        serverIsFullMessage =
            "The server is not accepting new connections, please try again later.";
        versionWrongMessage = "Unable to connect to the server. The game version is outdated.";
        inactiveMessage = "You have been logged out for inactivity.";
        disconnectedMessage = "You have been disconnected from the server.";
        disconnectCountMessage = "You will be disconnected from the server in ";
        connectionGenericErrorMessage = "Connection generic error.";
        roundInitMessage = "Round started!\nMake your move!";
        choiceMessage = "You chose ";
        opponentWaitMessage = "\nWaiting for the opponent's move...";
        winMessage = "ROUND WIN!";
        loseMessage = "ROUND LOSE!";
        drawMessage = "DRAW!";
        startingRoundMessage = "Starting a new round...";
        waitMessage = "Please wait.";
        matchWinMessage = "You WON the game!\nCongratulations!";
        matchLoseMessage = "You lost the game. :(\nDon't get discouraged, try again!";
        opponentCardMessage = "Your opponent chose ";
        backToMenuCountMessage = "Back to menu in ";
        genericErrorMessage = "Something went wrong, please try restarting the game.";
        roundTimeEndMessage = "TIME OUT!";
        iReallyWantToResetEverything = false;
        Debug.Log("Values reset successfully!");
    }
}

#if UNITY_EDITOR
[CustomEditor(typeof(GlobalConfigSO))]
public class GlobalConfigResetButton : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();
        var globalConfig = (GlobalConfigSO)target;
        if(GUILayout.Button("Reset Values"))
            globalConfig.ResetValues();
    }
}
#endif
