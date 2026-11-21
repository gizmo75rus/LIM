using System;

namespace AstmDriver.Commons.ASTM
{
    [AttributeUsage(AttributeTargets.Property)]
    internal class AstmDateTimeUsageAttribute : Attribute
    {
        public AstmDateTimeUsage DateTimeUsage { get; set; }

        public AstmDateTimeUsageAttribute(AstmDateTimeUsage dateTimeUsage)
        {
            DateTimeUsage = dateTimeUsage;
        }
    }
}
