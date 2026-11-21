using System;
using System.Runtime.Serialization;

namespace AstmDriver.Exceptions
{
    public class AstmProcessorException : Exception
    {
        public AstmProcessorException(string message, Exception innerException)
            : base(message, innerException)
        {
        }

        public AstmProcessorException(string message)
            : base(message)
        {
        }

        public AstmProcessorException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }

        public AstmProcessorException()
        {
        }
    }
}
