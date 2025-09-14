# DStream Counter Input Provider

A simple input provider for the DStream data streaming framework that generates sequential counter data for testing and validation.

## Overview

This provider generates:
- Sequential numbers (1, 2, 3, ...)
- Configurable intervals
- Rich metadata (timestamps, sequence numbers)
- JSON-structured envelopes

Perfect for testing output providers, validating data pipelines, and load testing.

## Usage

### Configuration

```hcl
task "test-counter" {
  input {
    provider_ref = "ghcr.io/katasec/dstream-counter-input-provider:latest"
    config {
      interval = 1000  # milliseconds between increments
      max_count = 100  # optional: stop after N items (0 = infinite)
    }
  }
  output {
    provider_ref = "ghcr.io/katasec/dstream-console-output-provider:latest"
    config {
      format = "json"
    }
  }
}
```

### Example Output

```json
{"payload": 1, "meta": {"seq": 1, "source": "counter", "timestamp": "2025-01-14T10:00:00Z"}}
{"payload": 2, "meta": {"seq": 2, "source": "counter", "timestamp": "2025-01-14T10:00:01Z"}}
{"payload": 3, "meta": {"seq": 3, "source": "counter", "timestamp": "2025-01-14T10:00:02Z"}}
```

## Architecture

- **Type**: Input Provider
- **Protocol**: gRPC via HashiCorp go-plugin
- **Framework**: .NET 9.0
- **Runtime**: Self-contained executable

## Building

```bash
dotnet publish -c Release -r osx-x64 -o out
```

## DStream Ecosystem

This provider is part of the DStream ecosystem:
- **CLI**: [DStream](https://github.com/katasec/dstream) - Go-based orchestration engine
- **SDK**: [DStream .NET SDK](https://github.com/katasec/dstream-dotnet-sdk) - .NET plugin development kit
- **Providers**: Independent, composable data sources and sinks

## Contributing

1. Fork this repository
2. Create a feature branch
3. Make your changes
4. Add tests
5. Submit a pull request

## License

MIT License - see LICENSE file for details.