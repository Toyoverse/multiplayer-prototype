using UnityEngine;

[CreateAssetMenu(fileName = "GlobalConfigSO", menuName = "ScriptableObjects/GlobalConfigSO", order = 0)]
public class GlobalConfigSO : ScriptableObject
{
    [Header("HEALTH")]
    public float maxHealth = 100;
    [Header("DAMAGE & COMBO")]
    public float baseDamage = 10;
    public float comboMultiplier = 0.5f;
    [Header("ROUNDS")] 
    public int maxInactiveRounds = 5;
    public float secondsPerRound = 10;
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
}
