using AstmDriver.Abstractions;

namespace AstmDriver.Commons.ASTM.Models
{
    public class StartingRange : AbstractAstmSubRecord
    {
        private string @_PatientID;

        private string @_SpecimenID;

        private string @_Reserved;

        [AstmRecordField(1)]
        public string PatientID
        {
            get
            {
                return @_PatientID;
            }
            set
            {
                @_PatientID = value;
            }
        }

        [AstmRecordField(2)]
        public string SpecimenID
        {
            get
            {
                return @_SpecimenID;
            }
            set
            {
                @_SpecimenID = value;
            }
        }

        [AstmRecordField(3)]
        public string Reserved
        {
            get
            {
                return @_Reserved;
            }
            set
            {
                @_Reserved = value;
            }
        }

        public StartingRange(string aLisString)
            : base(aLisString)
        {
        }

        public StartingRange()
        {
        }
    }
}
