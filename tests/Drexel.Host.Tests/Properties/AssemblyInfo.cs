using Microsoft.VisualStudio.TestTools.UnitTesting;

[assembly: DiscoverInternals]
[assembly: Parallelize(Scope = ExecutionScope.MethodLevel)]
[assembly: TestDataSourceDiscovery(TestDataSourceDiscoveryOption.DuringExecution)]