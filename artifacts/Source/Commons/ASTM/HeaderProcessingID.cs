namespace AstmDriver.Commons.ASTM
{
    public enum HeaderProcessingID
    {
        None,
        [AstmEnum("P")]
        Production,
        [AstmEnum("T")]
        Training,
        [AstmEnum("D")]
        Debugging,
        [AstmEnum("Q")]
        QualityControl
    }
}
