using System;
using System.Runtime.Serialization;

namespace AstmDriver.Exceptions
{
    public class AstmProcessorEstablishmentFailedException : AstmProcessorException
    {
        public AstmProcessorEstablishmentFailedException(string message, Exception innerException)
            : base(message, innerException)
        {
        }

        public AstmProcessorEstablishmentFailedException(string message)
            : base(message)
        {
        }

        public AstmProcessorEstablishmentFailedException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }

        public AstmProcessorEstablishmentFailedException()
        {
        }
    }
}
