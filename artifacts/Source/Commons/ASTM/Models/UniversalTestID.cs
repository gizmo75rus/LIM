using AstmDriver.Abstractions;

namespace AstmDriver.Commons.ASTM.Models
{
    public class UniversalTestID : AbstractAstmSubRecord
    {
        private string @_TestID;

        private string @_TestName;

        private string @_TestType;

        private string @_ManufacturerCode;

        private string[] @_OptionalFields;

        [AstmRecordField(1)]
        public string TestID
        {
            get
            {
                return @_TestID;
            }
            set
            {
                @_TestID = value;
            }
        }

        [AstmRecordField(2)]
        public string TestName
        {
            get
            {
                return @_TestName;
            }
            set
            {
                @_TestName = value;
            }
        }

        [AstmRecordField(3)]
        public string TestType
        {
            get
            {
                return @_TestType;
            }
            set
            {
                @_TestType = value;
            }
        }

        [AstmRecordField(4)]
        public string ManufacturerCode
        {
            get
            {
                return @_ManufacturerCode;
            }
            set
            {
                @_ManufacturerCode = value;
            }
        }

        [AstmRecordRemainingFields(5)]
        public string[] OptionalFields
        {
            get
            {
                return @_OptionalFields;
            }
            set
            {
                @_OptionalFields = value;
            }
        }

        public UniversalTestID(string aLisString)
            : base(aLisString)
        {
        }

        public UniversalTestID()
        {
        }
    }
}
