
public enum CARD_TYPE
{
    NONE = 0,
    ROCK = 1,
    PAPER = 2,
    SCISSOR = 3
}

public enum MESSAGE_TYPE
{
    NONE = 0,
    NEW_CONNECTION = 1,
    CARD_CHOICE = 2,
    ROUND_RESULT = 3,
    STRING = 4,
    START_GAME = 5,
    GAME_OVER = 6
}

public enum GAME_STATE
{
    NONE = 0,
    WAIT_CONNECTIONS = 1,
    STARTING = 2,
    CHOICE_TIME = 3,
    COMPARE_TIME = 4,
    GAME_OVER = 5
}
