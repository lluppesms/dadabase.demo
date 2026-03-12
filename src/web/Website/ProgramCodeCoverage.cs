// This file exists solely to apply [ExcludeFromCodeCoverage] to the
// compiler-synthesized Program class generated from top-level statements
// in Program.cs, preventing startup/infrastructure code from skewing
// unit-test coverage metrics.
[ExcludeFromCodeCoverage(Justification = "Program startup/infrastructure code is excluded from unit test coverage; end-to-end coverage is provided by Playwright tests.")]
partial class Program { }
