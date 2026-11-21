using AstmDriver.Abstractions;
using System;

namespace AstmDriver.Commons.ASTM.Models
{
    public class ResultRecord : AbstractAstmRecord
    {

        [AstmRecordField(2)]
        public int SequenceNumber { get; set; }

        [AstmRecordField(3)]
        public UniversalTestID UniversalTestID { get; set; }

        [AstmRecordField(4)]
        public string Data { get; set; }

        [AstmRecordField(5)]
        public string Units { get; set; }

        [AstmRecordField(6)]
        public string ReferenceRanges { get; set; }

        [AstmRecordField(7)]
        public ResultAbnormalFlags ResultAbnormalFlag { get; set; }

        [AstmRecordField(8)]
        public ResultNatureOfAbnormalityTestingSet NatureOfAbnormalityTesting { get; set; }

        [AstmRecordField(9)]
        public ResultStatus ResultStatus { get; set; }

        [AstmRecordField(11)]
        public string OperatorIdentification { get; set; }

        [AstmDateTimeUsage(AstmDateTimeUsage.DateTime)]
        [AstmRecordField(12)]
        public DateTime? TestStartedDateTime { get; set; }

        [AstmDateTimeUsage(AstmDateTimeUsage.DateTime)]
        [AstmRecordField(13)]
        public DateTime? TestCompletedDateTime { get; set; }

        public override string ToAstmString()
        {
            return "R" + new string(Delimiters.FieldDelimiter, 1) + base.ToAstmString();
        }

        public ResultRecord(string aLisString)
            : base(aLisString)
        {
        }

        public ResultRecord()
        {
        }
    }
}
