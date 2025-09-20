# DStream Counter Input Provider

A simple .NET input provider demonstrating clean architecture patterns in the DStream ecosystem. Generates sequential counter data with timestamps via stdin/stdout communication - perfect for testing output providers, validating data pipelines, and demonstrating the DStream .NET SDK.

## 📁 File Structure

```
counter-input-provider/
├── Program.cs    ← Top-level statement entry point (5 lines)
├── Config.cs     ← Configuration class (CounterConfig)
└── Reader.cs     ← Core data reading logic (ReadAsync implementation)
```

## 🎯 Architecture: Clean Separation of Concerns

This provider demonstrates **clean architecture patterns** for DStream input providers:

### 🔧 Core Data Reading (`Reader.cs`)

**Purpose**: Generate streaming data and emit envelopes  
**Interface**: `IInputProvider` from `Katasec.DStream.Abstractions`

```csharp
public interface IInputProvider : IProvider
{
    IAsyncEnumerable<Envelope> ReadAsync(IPluginContext ctx, CancellationToken ct);
}
```

**Required Implementation**:
- **`ReadAsync` method**: Returns `IAsyncEnumerable<Envelope>` for streaming data generation
- **`Envelope` structure**: `record struct Envelope(object Payload, IReadOnlyDictionary<string, object?> Meta)`
- **Error handling**: Respect `CancellationToken` for graceful shutdown
- **Streaming pattern**: Use `yield return` for continuous data generation

**Key Responsibilities**:
- ✅ Generate data streams (counters, CDC, API polling, file watching)
- ✅ Create `Envelope` objects with rich payload and metadata
- ✅ Handle timing/intervals for data generation
- ✅ Implement graceful shutdown on cancellation
- ✅ Add metadata for downstream routing and debugging

### 🔧 Configuration (`Config.cs`)

Simple configuration class with JSON binding attributes:

```csharp
public sealed record CounterConfig
{
    /// <summary>Interval in milliseconds between counter increments</summary>
    [JsonPropertyName("interval")]
    public int Interval { get; init; } = 1000;
    
    /// <summary>Maximum count before stopping (0 = infinite)</summary>
    [JsonPropertyName("max_count")]
    public int MaxCount { get; init; } = 0;
}
```

**Configuration Features**:
- ✅ Uses record types for immutable configuration
- ✅ JSON property name mapping for HCL/JSON compatibility
- ✅ Default values for all properties
- ✅ Clear documentation with XML comments

### 🚀 Entry Point (`Program.cs`)

Modern C# top-level statements:

```csharp
using Katasec.DStream.SDK.Core;
using CounterInputProvider;

// Top-level program entry point
await StdioProviderHost.RunInputProviderAsync<CounterInputProvider.CounterInputProvider, CounterInputProvider.CounterConfig>();
```

**What `StdioProviderHost` handles for you**:
- ✅ JSON configuration parsing from stdin
- ✅ Provider instantiation and initialization
- ✅ Envelope serialization to JSON for stdout
- ✅ Process lifecycle and graceful shutdown
- ✅ Error handling and logging to stderr

## 📋 Provider Development Checklist

### For Input Providers (Data Generation):

1. **✅ Inherit from `ProviderBase<TConfig>`**
2. **✅ Implement `IInputProvider`**
3. **✅ Implement `ReadAsync` method**:
   ```csharp
   public async IAsyncEnumerable<Envelope> ReadAsync(IPluginContext ctx, [EnumeratorCancellation] CancellationToken ct)
   {
       while (!ct.IsCancellationRequested)
       {
           // Your data generation logic here:
           // - Create data payload
           // - Add rich metadata for downstream processing
           // - Yield envelope for streaming
           
           var data = new { /* your data structure */ };
           var metadata = new Dictionary<string, object?> { /* routing/debug info */ };
           
           yield return new Envelope(data, metadata);
           
           // Wait/delay between items if needed
           await Task.Delay(Config.Interval, ct);
       }
   }
   ```

4. **✅ Use `[EnumeratorCancellation]` attribute** for proper cancellation handling

## 🧪 Testing

### Test Individual Provider
```bash
# Generate 3 counter items with 500ms intervals
echo '{"interval": 500, "max_count": 3}' | bin/Release/net9.0/osx-x64/counter-input-provider

# Infinite counter (stop with Ctrl+C)
echo '{"interval": 1000}' | bin/Release/net9.0/osx-x64/counter-input-provider

# Using dotnet run for development
echo '{"interval": 500, "max_count": 3}' | /usr/local/share/dotnet/dotnet run
```

### Test Full Pipeline
```bash
# Via DStream CLI
cd ~/progs/dstream/dstream
go run . run counter-to-console

# Manual pipeline testing
echo '{"interval": 500, "max_count": 3}' | ./counter-input-provider 2>/dev/null | \
echo '{"outputFormat": "simple"}' | ./console-output-provider
```

## 🏗️ Building

```bash
# Clean build using Makefile
make clean && make build

# Manual build  
/usr/local/share/dotnet/dotnet build -c Release
/usr/local/share/dotnet/dotnet publish -c Release -r osx-x64 --self-contained
```

## 📊 Data Output

### Example JSON Output
```json
{"source":"","type":"","data":{"value":1,"timestamp":"2025-09-20T17:05:56.424303+00:00"},"metadata":{"seq":1,"interval_ms":500,"provider":"counter-input-provider"}}
{"source":"","type":"","data":{"value":2,"timestamp":"2025-09-20T17:05:56.980548+00:00"},"metadata":{"seq":2,"interval_ms":500,"provider":"counter-input-provider"}}
{"source":"","type":"","data":{"value":3,"timestamp":"2025-09-20T17:05:57.492708+00:00"},"metadata":{"seq":3,"interval_ms":500,"provider":"counter-input-provider"}}
```

### Data Structure
- **`data.value`**: Sequential counter number (1, 2, 3, ...)
- **`data.timestamp`**: ISO 8601 timestamp when item was generated
- **`metadata.seq`**: Sequence number for debugging
- **`metadata.interval_ms`**: Configured interval for reference
- **`metadata.provider`**: Provider identification

## ⚙️ Configuration Options

Accepts JSON configuration via stdin:

```json
{
  "interval": 1000,
  "max_count": 100
}
```

**Configuration Properties**:
- **`interval`** (default: 1000): Milliseconds between counter increments
- **`max_count`** (default: 0): Maximum items to generate (0 = infinite)

## 🎯 Key Benefits of This Architecture

1. **🧩 Clear Separation**: Configuration, business logic, and entry point isolated
2. **🔧 Maintainable**: Easy to modify data generation logic in Reader.cs
3. **🧪 Testable**: Each component can be tested independently
4. **📦 Reusable**: Pattern works for any input provider (APIs, databases, files)
5. **⚡ Modern**: Uses latest C# patterns (top-level statements, records)

## 🔌 DStream Integration

### Task Configuration (HCL)
```hcl
task "counter-to-console" {
  input {
    provider_path = "./counter-input-provider"
    config = {
      interval = 1000
      max_count = 50
    }
  }
  output {
    provider_path = "./console-output-provider"
    config = {
      outputFormat = "structured"
    }
  }
}
```

### Pipeline Orchestration
```bash
# Run via DStream CLI
dstream run counter-to-console

# The CLI handles:
# 1. Launching input/output provider processes
# 2. Piping data: input.stdout → CLI → output.stdin
# 3. Configuration injection and process lifecycle
```

## 📖 Related Documentation

- [DStream .NET SDK](https://github.com/katasec/dstream-dotnet-sdk) - Main SDK documentation
- [Console Output Provider](https://github.com/katasec/dstream-console-output-provider) - Companion output provider
- [DStream CLI](https://github.com/katasec/dstream) - Go-based orchestration engine

## 🎪 Real-World Input Provider Examples

**This counter provider demonstrates patterns used in production providers**:

- **SQL Server CDC Provider**: ReadAsync → Monitor CDC tables, emit change events
- **REST API Provider**: ReadAsync → Poll endpoints, emit API responses  
- **File Watcher Provider**: ReadAsync → Watch directories, emit file change events
- **Kafka Consumer Provider**: ReadAsync → Consume topics, emit Kafka messages
- **Azure Service Bus Provider**: ReadAsync → Receive queue messages, emit envelopes

The same clean architecture and `ReadAsync` pattern applies regardless of the data source technology.

## 🚀 Getting Started

1. **Clone this repository** as a template for your input provider
2. **Modify `Config.cs`** with your provider's configuration needs
3. **Update `Reader.cs`** with your data generation logic
4. **Test independently** with echo/stdin before integrating with DStream CLI
5. **Build and distribute** as self-contained binary or OCI container

This provider serves as a **perfect template** for building your own DStream input providers with clean, maintainable architecture!