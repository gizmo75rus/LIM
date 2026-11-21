namespace AstmDriver.Commons.ASTM
{
    public enum OrderActionCode
    {
        None,
        [AstmEnum("C")]
        Cancel,
        [AstmEnum("A")]
        Add,
        [AstmEnum("N")]
        New,
        [AstmEnum("P")]
        Pending,
        [AstmEnum("L")]
        Reserved,
        [AstmEnum("X")]
        InProcess,
        [AstmEnum("Q")]
        QC
    }
}
