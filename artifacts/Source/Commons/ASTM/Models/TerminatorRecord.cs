using AstmDriver.Abstractions;

namespace AstmDriver.Commons.ASTM.Models
{
    public class TerminatorRecord : AbstractAstmRecord
    {
        private int @_SequenceNumber = 1;

        private TerminationCode @_TerminationCode;

        [AstmRecordField(2)]
        public int SequenceNumber
        {
            get
            {
                return @_SequenceNumber;
            }
            set
            {
                @_SequenceNumber = value;
            }
        }

        [AstmRecordField(3)]
        public TerminationCode TerminationCode
        {
            get
            {
                return @_TerminationCode;
            }
            set
            {
                @_TerminationCode = value;
            }
        }

        public override string ToAstmString()
        {
            return "L" + new string(Delimiters.FieldDelimiter, 1) + base.ToAstmString();
        }

        public TerminatorRecord(string aLisString)
            : base(aLisString)
        {
        }

        public TerminatorRecord()
        {
        }
    }
}
