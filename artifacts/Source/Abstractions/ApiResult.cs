namespace AstmDriver.Abstractions
{
    public abstract class ApiResult
    {
        public bool Ok { get; set; }
        public string ErrorMessage { get; set; }
    }

    public abstract class ApiResult<T> : ApiResult where T : class
    {
        public T Value { get; set; }
    }
}
