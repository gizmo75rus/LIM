using System;
using System.Collections.Generic;

namespace AstmDriver.Commons.ASTM
{
    public class Message
    {
        public Guid Guid { get; set; }
        public DateTime CreatedAt { get; set; }

        public List<string> RawData { get; set; }
        public List<string> Records { get; set; }

        public bool IsQuery { get; set; }
        public bool IsResult { get; set; }
        public bool IsOrder { get; set; }

        public Message()
        {
            Guid = Guid.NewGuid();
            CreatedAt = DateTime.Now;
            IsQuery = false;
            IsResult = false;
            Records = new List<string>();
            RawData = new List<string>();
        }
    }
}
