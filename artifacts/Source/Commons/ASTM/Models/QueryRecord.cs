using AstmDriver.Abstractions;

namespace AstmDriver.Commons.ASTM.Models
{
    public class QueryRecord : AbstractAstmRecord
    {

        [AstmRecordField(2)]
        public int SequenceNumber { get; set; }

        [AstmRecordField(3)]
        public StartingRange StartingRange { get; set; }

        [AstmRecordField(4)]
        public StartingRange EndingRange { get; set; }

        [AstmRecordField(5)]
        public UniversalTestID UniversalTestID { get; set; }

        public override string ToAstmString()
        {
            return "Q" + new string(Delimiters.FieldDelimiter, 1) + base.ToAstmString();
        }

        public QueryRecord(string aLisString)
            : base(aLisString)
        {
        }

        public QueryRecord()
        {
        }
    }
}
