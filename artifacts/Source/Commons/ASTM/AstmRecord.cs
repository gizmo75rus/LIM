using AstmDriver.Abstractions;

namespace AstmDriver.Commons.ASTM
{
    public class AstmRecord
    {
        public AstmRecordType Type { get; set; }
        public AbstractAstmRecord Record { get; set; }

        public AstmRecord(AstmRecordType type, AbstractAstmRecord record)
        {
            Type = type;
            Record = record;
        }

    }
}
