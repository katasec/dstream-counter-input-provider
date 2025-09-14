using Katasec.DStream.SDK.PluginHost;
using Katasec.DStream.Abstractions;
using HCLog.Net;
using Katasec.DStream.SDK.Core;
using System.Runtime.CompilerServices;

// DStream Counter Input Provider
// Generates sequential counter data for testing and validation
await PluginHost.Run<CounterInputProvider, CounterConfig>();

/// <summary>
/// Configuration for the Counter Input Provider
/// </summary>
public sealed record CounterConfig
{
    /// <summary>Interval in milliseconds between counter increments</summary>
    public int Interval { get; init; } = 1000;
    
    /// <summary>Maximum count before stopping (0 = infinite)</summary>
    public int MaxCount { get; init; } = 0;
}

/// <summary>
/// Counter Input Provider - generates sequential counter data
/// Perfect for testing output providers and validating pipelines
/// </summary>
public sealed class CounterInputProvider : ProviderBase<CounterConfig>, IInputProvider
{
    public async IAsyncEnumerable<Envelope> ReadAsync(
        IPluginContext ctx, 
        [EnumeratorCancellation] CancellationToken ct)
    {
        var logger = (HCLogger)ctx.Logger;
        logger.Info($"counter_input_start interval={Config.Interval}ms max_count={Config.MaxCount}");

        var count = 0;
        while (!ct.IsCancellationRequested)
        {
            count++;
            
            // Stop if max count reached
            if (Config.MaxCount > 0 && count > Config.MaxCount)
            {
                logger.Info($"counter_input_complete reached_max_count={Config.MaxCount}");
                break;
            }

            // Create envelope with counter data and metadata
            var meta = new Dictionary<string, object?>
            {
                ["seq"] = count,
                ["source"] = "counter",
                ["timestamp"] = DateTimeOffset.UtcNow.ToString("O"),
                ["interval_ms"] = Config.Interval
            };

            var envelope = new Envelope(count, meta);
            logger.Info($"counter_emit seq={count} payload={count}");
            
            yield return envelope;

            // Wait for configured interval
            try
            {
                await Task.Delay(Config.Interval, ct);
            }
            catch (OperationCanceledException)
            {
                logger.Info("counter_input_cancelled");
                break;
            }
        }
        
        logger.Info($"counter_input_stopped final_count={count}");
    }
}
