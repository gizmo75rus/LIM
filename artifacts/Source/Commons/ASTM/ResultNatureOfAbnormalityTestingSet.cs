using System;

namespace AstmDriver.Commons.ASTM
{
    [Flags]
    public enum ResultNatureOfAbnormalityTestingSet
    {
        None = 0x0,
        [AstmEnum("A")]
        Age = 0x1,
        [AstmEnum("S")]
        Sex = 0x2,
        [AstmEnum("R")]
        Race = 0x4,
        [AstmEnum("N")]
        Normal = 0x8
    }
}
