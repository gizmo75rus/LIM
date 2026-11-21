using AstmDriver.Abstractions;
using System;

namespace AstmDriver.Commons.ASTM.Models
{
    public class PatientRecord : AbstractAstmRecord
    {

        [AstmRecordField(2)]
        public int SequenceNumber { get; set; }

        [AstmRecordField(3)]
        public string PracticeAssignedPatientID { get; set; }

        [AstmRecordField(4)]
        public string LaboratoryAssignedPatientID { get; set; }

        [AstmRecordField(5)]
        public string PatientID3 { get; set; }

        [AstmRecordField(6)]
        public PatientName PatientName { get; set; }

        [AstmRecordField(7)]
        public PatientName MothersMaidenName { get; set; }

        [AstmDateTimeUsage(AstmDateTimeUsage.Date)]
        [AstmRecordField(8)]
        public DateTime? Birthdate { get; set; } = null;

        [AstmRecordField(9)]
        public PatientSex? PatientSex { get; set; } = null;

        [AstmRecordField(14)]
        public string AttendingPhysicianID { get; set; }

        public override string ToAstmString()
        {
            return "P" + new string(Delimiters.FieldDelimiter, 1) + base.ToAstmString();
        }

        public PatientRecord(string aLisString)
            : base(aLisString)
        {
        }

        public PatientRecord()
        {
        }
    }
}
