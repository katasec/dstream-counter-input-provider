using DStream.Providers.CounterInput;
using Katasec.DStream.SDK.Core;

await StdioProviderHost.RunInputProviderAsync<CounterInputProvider, CounterConfig>();
