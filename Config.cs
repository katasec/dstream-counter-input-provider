using System.Text.Json.Serialization;

namespace CounterInputProvider;

/// <summary>
/// Configuration for the Counter Input Provider
/// </summary>
public sealed record CounterConfig
{
    /// <summary>Interval in milliseconds between counter increments</summary>
    [JsonPropertyName("interval")]
    public int Interval { get; init; } = 1000;
    
    /// <summary>Maximum count before stopping (0 = infinite)</summary>
    [JsonPropertyName("max_count")]
    public int MaxCount { get; init; } = 0;
}