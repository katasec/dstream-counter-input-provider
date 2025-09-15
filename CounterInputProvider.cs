using System.Runtime.CompilerServices;
using Katasec.DStream.Abstractions;
using Katasec.DStream.SDK.Core;

namespace DStream.Providers.CounterInput;

/// <summary>
/// Simple Counter Input Provider Example
/// Generates sequential counter data perfect for testing output providers
/// </summary>
public sealed class CounterInputProvider : ProviderBase<CounterConfig>, IInputProvider
{
    public async IAsyncEnumerable<Envelope> ReadAsync(IPluginContext ctx, [EnumeratorCancellation] CancellationToken ct)
    {
        var count = 0;
        
        while (!ct.IsCancellationRequested)
        {
            count++;
            
            // Stop if max count reached
            if (Config.MaxCount > 0 && count > Config.MaxCount)
            {
                break;
            }

            // Create counter data
            var data = new
            {
                value = count,
                timestamp = DateTimeOffset.UtcNow.ToString("O")
            };
            
            var metadata = new Dictionary<string, object?>
            {
                ["seq"] = count,
                ["interval_ms"] = Config.Interval,
                ["provider"] = "counter-input-provider"
            };
            
            yield return new Envelope(data, metadata);
            
            // Wait for configured interval
            try
            {
                await Task.Delay(Config.Interval, ct);
            }
            catch (OperationCanceledException)
            {
                break;
            }
        }
    }
}