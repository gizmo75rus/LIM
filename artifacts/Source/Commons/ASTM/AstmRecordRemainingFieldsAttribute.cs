using System;

namespace AstmDriver.Commons.ASTM
{
    [AttributeUsage(AttributeTargets.Property)]
    internal class AstmRecordRemainingFieldsAttribute : AstmRecordFieldAttribute
    {
        public AstmRecordRemainingFieldsAttribute(int aFieldIndex)
            : base(aFieldIndex)
        {
        }
    }
}
