using Katasec.DStream.SDK.Core;
using CounterInputProvider;

// Top-level program entry point
await StdioProviderHost.RunInputProviderAsync<CounterInputProvider.CounterInputProvider, CounterInputProvider.CounterConfig>();
