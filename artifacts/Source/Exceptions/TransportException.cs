using System;
using System.Runtime.Serialization;

namespace AstmDriver.Exceptions
{
    public class TransportException : HostConnectionException
    {
        public TransportException()
        {
        }

        public TransportException(string message) : base(message)
        {
        }

        public TransportException(string message, Exception innerException) : base(message, innerException)
        {
        }

        public TransportException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}
