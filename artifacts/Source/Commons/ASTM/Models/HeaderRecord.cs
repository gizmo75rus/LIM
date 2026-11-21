
using AstmDriver.Abstractions;
using System;

namespace AstmDriver.Commons.ASTM.Models
{
    public class HeaderRecord : AbstractAstmRecord
    {

        [AstmRecordField(3)]
        public string MessageControlID { get; set; }

        [AstmRecordField(4)]
        public string AccessPassword { get; set; }

        [AstmRecordField(5)]
        public string SenderID { get; set; }

        [AstmRecordField(6)]
        public string SenderStreetAddress { get; set; }

        [AstmRecordField(8)]
        public string SenderTelephoneNumber { get; set; }

        [AstmRecordField(9)]
        public string CharacteristicsOfSender { get; set; }

        [AstmRecordField(10)]
        public string ReceiverID { get; set; }

        [AstmRecordField(11)]
        public string Comment { get; set; }

        [AstmRecordField(12)]
        public HeaderProcessingID ProcessingID { get; set; }

        [AstmRecordField(13)]
        public string Version { get; set; } = "LIS2-A2";

        [AstmRecordField(14)]
        public DateTime MessageDateTime { get; set; } = DateTime.Now;

        public override string ToAstmString()
        {
            return "H" + new string(Delimiters.FieldDelimiter, 1) + new string(Delimiters.RepeatDelimiter, 1) + new string(Delimiters.ComponentDelimiter, 1) + new string(Delimiters.EscapeCharacter, 1) + new string(Delimiters.FieldDelimiter, 1) + base.ToAstmString();
        }

        public HeaderRecord(string aLisString) : base(aLisString)
        {
            Delimiters.FieldDelimiter = aLisString[1];
            Delimiters.RepeatDelimiter = aLisString[2];
            Delimiters.ComponentDelimiter = aLisString[3];
            Delimiters.EscapeCharacter = aLisString[4];
        }

        public HeaderRecord()
        {
        }
    }
}
