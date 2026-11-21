namespace AstmDriver.Commons.ASTM
{
    public enum OrderReportType
    {
        None,
        [AstmEnum("O")]
        Order,
        [AstmEnum("C")]
        Correction,
        [AstmEnum("P")]
        Preliminary,
        [AstmEnum("F")]
        Final,
        [AstmEnum("X")]
        Cancelled,
        [AstmEnum("I")]
        Pending,
        [AstmEnum("Y")]
        NoOrder,
        [AstmEnum("Z")]
        NoRecord,
        [AstmEnum("Q")]
        Response
    }
}
