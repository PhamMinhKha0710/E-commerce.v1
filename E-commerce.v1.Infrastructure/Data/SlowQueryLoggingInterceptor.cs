using System.Data.Common;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Logging;

namespace E_commerce.v1.Infrastructure.Data;

/// <summary>
/// Logs EF Core DbCommand execution durations, warning on slow queries so we
/// can validate performance improvements in dev/staging without opening a
/// full APM tool.
/// </summary>
public class SlowQueryLoggingInterceptor : DbCommandInterceptor
{
    private static readonly TimeSpan SlowThreshold = TimeSpan.FromMilliseconds(300);

    private readonly ILogger<SlowQueryLoggingInterceptor> _logger;

    public SlowQueryLoggingInterceptor(ILogger<SlowQueryLoggingInterceptor> logger)
    {
        _logger = logger;
    }

    public override ValueTask<DbDataReader> ReaderExecutedAsync(
        DbCommand command,
        CommandExecutedEventData eventData,
        DbDataReader result,
        CancellationToken cancellationToken = default)
    {
        LogIfSlow(command, eventData.Duration);
        return base.ReaderExecutedAsync(command, eventData, result, cancellationToken);
    }

    public override ValueTask<int> NonQueryExecutedAsync(
        DbCommand command,
        CommandExecutedEventData eventData,
        int result,
        CancellationToken cancellationToken = default)
    {
        LogIfSlow(command, eventData.Duration);
        return base.NonQueryExecutedAsync(command, eventData, result, cancellationToken);
    }

    public override ValueTask<object?> ScalarExecutedAsync(
        DbCommand command,
        CommandExecutedEventData eventData,
        object? result,
        CancellationToken cancellationToken = default)
    {
        LogIfSlow(command, eventData.Duration);
        return base.ScalarExecutedAsync(command, eventData, result, cancellationToken);
    }

    private void LogIfSlow(DbCommand command, TimeSpan duration)
    {
        if (duration >= SlowThreshold)
        {
            _logger.LogWarning(
                "Slow SQL ({ElapsedMs} ms): {CommandText}",
                (long)duration.TotalMilliseconds,
                command.CommandText);
        }
    }
}
