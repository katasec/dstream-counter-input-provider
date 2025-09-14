# DStream Counter Input Provider

A simple .NET input provider for the DStream data streaming framework that generates sequential counter data via stdin/stdout communication. Perfect for testing output providers, validating data pipelines, and demonstrating the DStream .NET SDK.

## Overview

This provider generates:
- Sequential numbers (1, 2, 3, ...) with timestamps
- Configurable intervals between increments
- Rich metadata (sequence numbers, provider info)
- JSON-structured data envelopes
- Demonstrates minimal code needed for a DStream input provider

## Configuration

Accepts JSON configuration via stdin:

```json
{
  "interval": 1000,
  "max_count": 100
}
```

**Options:**
- `interval`: Milliseconds between counter increments (default: 1000)
- `max_count`: Optional limit, stops after N items (default: 0 = infinite)

## Usage

### Standalone Testing

```bash
# Generate 3 counter items with 500ms interval
echo '{"interval": 500, "max_count": 3}' | /usr/local/share/dotnet/dotnet run
```

### Example Output

```json
{"source":"","type":"","data":{"value":1,"timestamp":"2025-09-14T17:11:21.5590040+00:00"},"metadata":{"seq":1,"interval_ms":500,"provider":"counter-input-provider"}}
{"source":"","type":"","data":{"value":2,"timestamp":"2025-09-14T17:11:22.9125080+00:00"},"metadata":{"seq":2,"interval_ms":500,"provider":"counter-input-provider"}}
{"source":"","type":"","data":{"value":3,"timestamp":"2025-09-14T17:11:23.4208450+00:00"},"metadata":{"seq":3,"interval_ms":500,"provider":"counter-input-provider"}}
```

### Pipeline Testing

```bash
# Test full pipeline with console output provider
echo '{"interval": 500, "max_count": 3}' | /usr/local/share/dotnet/dotnet run 2>/dev/null | \
echo '{"outputFormat": "simple"}' | ../dstream-console-output-provider/console-output-provider
```

## Implementation

### Complete Provider Code

```csharp
using System.Runtime.CompilerServices;
using System.Text.Json.Serialization;
using Katasec.DStream.Abstractions;
using Katasec.DStream.SDK.Core;

// Simple top-level program entry point - uses SDK infrastructure
await StdioProviderHost.RunInputProviderAsync<CounterInputProvider, CounterConfig>();

public class CounterInputProvider : ProviderBase<CounterConfig>, IInputProvider
{
    public async IAsyncEnumerable<Envelope> ReadAsync(IPluginContext ctx, [EnumeratorCancellation] CancellationToken ct)
    {
        var count = 0;
        
        while (!ct.IsCancellationRequested)
        {
            count++;
            
            // Stop if max count reached
            if (Config.MaxCount > 0 && count > Config.MaxCount)
                break;

            // Create counter data
            var data = new { value = count, timestamp = DateTimeOffset.UtcNow };
            var metadata = new Dictionary<string, object?>
            {
                ["seq"] = count,
                ["interval_ms"] = Config.Interval,
                ["provider"] = "counter-input-provider"
            };
            
            yield return new Envelope(data, metadata);
            
            await Task.Delay(Config.Interval, ct);
        }
    }
}

public sealed record CounterConfig
{
    [JsonPropertyName("interval")]
    public int Interval { get; init; } = 1000;
    
    [JsonPropertyName("max_count")]
    public int MaxCount { get; init; } = 0;
}
```

## Architecture

- **Type**: Input Provider (generates data)
- **Protocol**: stdin/stdout JSON communication
- **Framework**: .NET 9.0 with DStream .NET SDK
- **Runtime**: Self-contained executable

### SDK Benefits

**What the SDK handles:**
- JSON configuration parsing and binding
- stdin/stdout communication protocol
- Process lifecycle and graceful shutdown
- Envelope serialization to JSON
- Error handling and logging

**What you focus on:**
- Data generation logic (counter with intervals)
- Configuration model
- Business logic (when to stop, what metadata to include)

## Building

```bash
# Build debug version (PowerShell on macOS)
/usr/local/share/dotnet/dotnet build

# Build release version  
/usr/local/share/dotnet/dotnet build -c Release

# Publish self-contained binary
/usr/local/share/dotnet/dotnet publish -c Release -r osx-x64 --self-contained
```

## DStream Integration

### Task Configuration (HCL)

```hcl
task "counter-to-console" {
  input {
    provider_path = "./counter-input-provider"
    config = {
      interval = 1000
      max_count = 10
    }
  }
  output {
    provider_path = "./console-output-provider"
    config = {
      outputFormat = "simple"
    }
  }
}
```

## DStream Ecosystem

This provider is part of the DStream ecosystem:
- **CLI**: [DStream](https://github.com/katasec/dstream) - Go-based orchestration engine
- **SDK**: [DStream .NET SDK](https://github.com/katasec/dstream-dotnet-sdk) - .NET provider development kit
- **Providers**: Independent, composable data sources and sinks

This demonstrates how simple it is to create a DStream input provider - the complete implementation is ~50 lines of code thanks to the SDK infrastructure!
