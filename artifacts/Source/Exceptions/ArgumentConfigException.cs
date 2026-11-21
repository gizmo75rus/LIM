using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AstmDriver.Exceptions
{
    public class ArgumentConfigException : Exception
    {
        public ArgumentConfigException()
        {

        }

        public ArgumentConfigException(string message) : base(message)
        {

        }
    }
}
