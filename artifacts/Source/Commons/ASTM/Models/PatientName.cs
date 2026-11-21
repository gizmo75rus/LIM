using AstmDriver.Abstractions;
using System.Runtime.CompilerServices;

namespace AstmDriver.Commons.ASTM.Models
{
    public class PatientName : AbstractAstmSubRecord
    {
        [CompilerGenerated]
        private string @_LastName;

        [CompilerGenerated]
        private string @_FirstName;

        [CompilerGenerated]
        private string @_MiddleName;

        [CompilerGenerated]
        private string @_Suffix;

        [CompilerGenerated]
        private string @_Title;

        [AstmRecordField(1)]
        public string LastName
        {
            get
            {
                return @_LastName;
            }
            set
            {
                @_LastName = value;
            }
        }

        [AstmRecordField(2)]
        public string FirstName
        {
            get
            {
                return @_FirstName;
            }
            set
            {
                @_FirstName = value;
            }
        }

        [AstmRecordField(3)]
        public string MiddleName
        {
            get
            {
                return @_MiddleName;
            }
            set
            {
                @_MiddleName = value;
            }
        }

        [AstmRecordField(4)]
        public string Suffix
        {
            get
            {
                return @_Suffix;
            }
            set
            {
                @_Suffix = value;
            }
        }

        [AstmRecordField(5)]
        public string Title
        {
            get
            {
                return @_Title;
            }
            set
            {
                @_Title = value;
            }
        }

        public PatientName(string aLisString)
            : base(aLisString)
        {
        }

        public PatientName()
        {
        }
    }
}
