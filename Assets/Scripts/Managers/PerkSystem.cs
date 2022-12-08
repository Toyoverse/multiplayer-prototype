
public static class PerkSystem 
{
    public static CARD_TYPE GetWeakestType(CARD_TYPE cardType)
    {
        var result = cardType switch
        {
            CARD_TYPE.BOND => CARD_TYPE.DEFENSE,
            CARD_TYPE.ATTACK => CARD_TYPE.BOND,
            CARD_TYPE.DEFENSE => CARD_TYPE.ATTACK,
            _ => CARD_TYPE.NONE
        };
        return result;
    }
    
    public static CARD_TYPE GetStrongestType(CARD_TYPE cardType)
    {
        var result = cardType switch
        {
            CARD_TYPE.BOND => CARD_TYPE.ATTACK,
            CARD_TYPE.ATTACK => CARD_TYPE.DEFENSE,
            CARD_TYPE.DEFENSE => CARD_TYPE.BOND,
            _ => CARD_TYPE.NONE
        };
        return result;
    }
}
