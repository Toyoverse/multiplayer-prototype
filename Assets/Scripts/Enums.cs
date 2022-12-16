
public enum CARD_TYPE
{
    EMPTY = 0,
    NONE = 1,
    BOND = 2, //ROCK
    DEFENSE = 3, //PAPER
    ATTACK = 4 //SCISSOR
}

public enum MESSAGE_TYPE
{
    NONE = 0,
    NEW_CONNECTION = 1,
    CARD_CHOICE = 2,
    ROUND_RESULT = 3,
    STRING = 4,
    START_GAME = 5,
    GAME_OVER = 6,
    CONNECTION_REFUSE = 7
}

public enum SERVER_STATE
{
    NONE = 0,
    WAIT_CONNECTIONS = 1,
    STARTING = 2,
    CHOICE_TIME = 3,
    COMPARE_TIME = 4,
    GAME_OVER = 5,
}

public enum SIMPLE_RESULT
{
    NONE = 0,
    LOSE = 1,
    DRAW = 2,
    WIN = 3
}

public enum LOCAL_STATE
{
    NONE = 0,
    MENU = 1,
    GAMEPLAY = 2
}

public enum PLAYABLE_TYPE
{
    NONE = 0,
    DAMAGE = 1,
    KNOCKBACK = 2,
    DRAW = 3,
    VICTORY = 4,
    DEFEAT = 5
}

public enum KICK_REASON
{
    NONE = 0,
    SERVER_IS_FULL = 1,
    WRONG_VERSION = 2,
    INACTIVE = 3
}