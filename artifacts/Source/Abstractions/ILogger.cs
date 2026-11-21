namespace AstmDriver.Abstractions
{
    public interface ILogger
    {
        string TypeName { get; set; }
        void Error(System.Exception ex, string error);
        void Debug(string message);
        void Info(string message);
        void Trace(string traceMessage);
        void Error(string message);
        void WriteCrashDump(string data);
    }

}
