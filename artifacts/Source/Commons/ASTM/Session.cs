using System;
using System.Collections.Generic;

namespace AstmDriver.Commons.ASTM
{
    public class Session
    {
        public DateTime CreatedAt { get; set; }
        public List<AstmRecord> Records { get; set; }
        public Session()
        {
            CreatedAt = DateTime.Now;
            Records = new List<AstmRecord>();
        }

    }
}
