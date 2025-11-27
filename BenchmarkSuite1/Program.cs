using BenchmarkDotNet.Running;
using BenchmarkSuite1;

BenchmarkSwitcher.FromAssembly(typeof(CountriesCacheBenchmark).Assembly).Run(args);
