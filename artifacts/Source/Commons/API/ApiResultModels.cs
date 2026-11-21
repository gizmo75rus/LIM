using AstmDriver.Abstractions;

namespace AstmDriver.Commons.API
{
    public class Ok : ApiResult
    {
        public Ok()
        {
            Ok = true;
        }
    }

    public class Error : ApiResult
    {
        public Error(string message)
        {
            Ok = false;
            ErrorMessage = message;
        }
    }

    public class Ok<T> : ApiResult<T> where T : class
    {
        public Ok(T value)
        {
            Ok = true;
            Value = value;
        }
    }

    public class Error<T> : ApiResult<T> where T : class
    {
        public Error(string message)
        {
            Ok = false;
            ErrorMessage = message;
        }
    }
}
