using AstmDriver.Abstractions;
using System;

namespace AstmDriver.Commons.ASTM.Models
{
    public class OrderRecord : AbstractAstmRecord
    {

        [AstmRecordField(2)]
        public int SequenceNumber { get; set; }
        [AstmRecordField(3)]
        public string SpecimenID { get; set; }

        //8.4.4 Instrument Specimen ID
        //This text field shall represent a unique identifier assigned by the instrument, if different from the
        //information system identifier, and returned with results for use in referring to any results.
        [AstmRecordField(4)]
        public string InstrumentSpecimenID { get; set; }

        [AstmRecordField(5)]
        public UniversalTestID TestID { get; set; }

        [AstmRecordField(6)]
        public OrderPriority Priority { get; set; }

        [AstmDateTimeUsage(AstmDateTimeUsage.DateTime)]
        [AstmRecordField(7)]
        public DateTime? RequestedDateTime { get; set; }

        //8.4.8 Specimen Collection Date and Time
        //This field shall represent the actual time the specimen was collected or obtained.
        [AstmDateTimeUsage(AstmDateTimeUsage.DateTime)]
        [AstmRecordField(8)]
        public DateTime? SpecimenCollectionDateTime { get; set; }

        //8.4.9 Collection End Time
        //This field shall contain the end date and time of a timed specimen collection, such as 24-hour urine
        //collection.
        [AstmDateTimeUsage(AstmDateTimeUsage.DateTime)]
        [AstmRecordField(9)]
        public DateTime? CollectionEndDateTime { get; set; }



        [AstmRecordField(12)]
        public OrderActionCode ActionCode { get; set; }

        [AstmRecordField(26)]
        public OrderReportType ReportType { get; set; }

        public override string ToAstmString()
        {
            return "O" + new string(Delimiters.FieldDelimiter, 1) + base.ToAstmString();
        }

        public OrderRecord(string aLisString)
            : base(aLisString)
        {
        }

        public OrderRecord()
        {
        }
    }
}
