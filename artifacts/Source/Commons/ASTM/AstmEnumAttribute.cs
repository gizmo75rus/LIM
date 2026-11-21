using System;

namespace AstmDriver.Commons.ASTM
{
    [AttributeUsage(AttributeTargets.Field)]
    internal class AstmEnumAttribute : Attribute
    {

        public string LisID { get; set; }

        public AstmEnumAttribute(string aLisID)
        {
            LisID = aLisID;
        }
    }
}
