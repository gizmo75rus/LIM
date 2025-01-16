using LIM.SharedKernel.Interfaces;
using Microsoft.Extensions.Logging;

namespace LIM.ApplicationCore.BaseObjects;

public abstract class AbstractService : IService
{
    protected const int CtsLiveTimeMs = 10000; // 10 000ms = 10s
    
    protected CancellationTokenSource _cts 
        = new CancellationTokenSource(CtsLiveTimeMs);
    
    protected ILogger _logger;
}