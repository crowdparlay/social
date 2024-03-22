// MaxParallelThreads = -1 is required to prevent xUnit deadlock issue when running tests in environments with low Environment.ProcessorCount.
// See: https://github.com/xunit/xunit/issues/1747
[assembly: CollectionBehavior(MaxParallelThreads = -1)]