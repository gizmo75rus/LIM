using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace AstmDriver.Exceptions
{
    public class HostConnectionException : Exception
    {
        public HostConnectionException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }

        public HostConnectionException(string message, Exception innerException) : base(message, innerException)
        {
        }

        public HostConnectionException(string message) : base(message)
        {
        }

        public HostConnectionException()
        {
        }
    }
}
