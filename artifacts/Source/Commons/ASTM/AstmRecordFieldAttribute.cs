using System;
using System.Runtime.CompilerServices;

namespace AstmDriver.Commons.ASTM
{
    [AttributeUsage(AttributeTargets.Property)]
    internal class AstmRecordFieldAttribute : Attribute
    {
        private int @_FieldIndex;

        public int FieldIndex
        {
            get
            {
                return @_FieldIndex;
            }
            set
            {
                @_FieldIndex = value;
            }
        }

        public AstmRecordFieldAttribute(int aFieldIndex)
        {
            @_FieldIndex = aFieldIndex;
        }
    }
}
