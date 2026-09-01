using Serilog;
using VmcHmi.Application.Interfaces;

namespace VmcHmi.Infrastructure.Logging;

public class AppLogger<T> : IAppLogger<T>
{
    private readonly ILogger _logger;

    public AppLogger()
    {
        _logger = Log.ForContext<T>();
    }

    public void LogInformation(string message, params object[] args)
    {
        _logger.Information(message, args);
    }

    public void LogWarning(string message, params object[] args)
    {
        _logger.Warning(message, args);
    }

    public void LogError(Exception ex, string message, params object[] args)
    {
        _logger.Error(ex, message, args);
    }

    public void LogError(string message, params object[] args)
    {
        _logger.Error(message, args);
    }
}
