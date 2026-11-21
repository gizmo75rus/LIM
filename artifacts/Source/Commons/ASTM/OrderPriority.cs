namespace AstmDriver.Commons.ASTM
{
    public enum OrderPriority
    {
        None,
        [AstmEnum("S")]
        Stat,
        [AstmEnum("A")]
        ASAP,
        [AstmEnum("R")]
        Routine,
        [AstmEnum("C")]
        CallBack,
        [AstmEnum("P")]
        PreOperative
    }
}
