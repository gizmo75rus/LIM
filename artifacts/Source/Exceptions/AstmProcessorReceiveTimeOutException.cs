using System;
using System.Runtime.Serialization;

namespace AstmDriver.Exceptions
{
    public class AstmProcessorReceiveTimeOutException : AstmProcessorException
    {
        public AstmProcessorReceiveTimeOutException(string message, Exception innerException)
            : base(message, innerException)
        {
        }

        public AstmProcessorReceiveTimeOutException(string message)
            : base(message)
        {
        }

        public AstmProcessorReceiveTimeOutException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }

        public AstmProcessorReceiveTimeOutException()
        {
        }
    }
}
