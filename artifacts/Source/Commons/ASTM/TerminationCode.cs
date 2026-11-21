namespace AstmDriver.Commons.ASTM
{
    public enum TerminationCode
    {
        [AstmEnum("N")]
        Normal,
        [AstmEnum("T")]
        SenderAborted,
        [AstmEnum("R")]
        ReceiverRequestedAbort,
        [AstmEnum("E")]
        UnknownSystemError,
        [AstmEnum("Q")]
        ErrorInLastRequestForInformation,
        [AstmEnum("I")]
        NoInformationAvailableFromLastQuery,
        [AstmEnum("F")]
        LastRequestForInformationProcessed
    }
}
