---
agent: sw-tester
model: Claude Haiku 4.5 (copilot)
name: test-for-quality
description: Execute comprehensive software quality assurance (QA) checks including automated test generation, edge case analysis, test data creation, bug reproduction, regression testing, coverage improvement, and automation scripting aligned with SW Tester/QA Engineer personas.
---

# Comprehensive Software Quality Assurance Checks

**Version:** 2.0 (Enhanced with real-world requirements)  
**Last Updated:** January 29, 2026  
**Phases:** 17 | **Checks:** 50+ | **Scope:** Enterprise-Grade Quality

## Overview

This comprehensive quality assurance prompt provides a **structured workflow with 17 distinct phases** to validate that the calculator solution meets enterprise-grade software quality standards. Tailored for SW Tester/QA Engineer personas, it encompasses:

- **Code Quality** – Static analysis, complexity metrics, naming standards
- **Testing Rigor** – Automated test generation, edge cases, coverage improvement
- **QA Engineering** – Test data generation, bug reproduction, regression testing, automation
- **Security** – Vulnerability scanning, code security review, dependency analysis
- **Performance** – Build time, test execution, runtime optimization
- **Documentation** – API docs, README, commit message quality
- **Compliance** – Framework standards, project structure, configuration validation
- **Integration** – End-to-end testing, full solution verification

---

## Azure DevOps Work Item Integration

### Epic #82: Create .NET 8 Console Calculator with xUnit Testing

**Status:** New | **Priority:** High  
**Project:** autocloudarc-mcaps / create-dotnet-calculator  
**Link:** [View Epic #82](https://dev.azure.com/autocloudarc-mcaps/create-dotnet-calculator/_backlogs/backlog/)

This QA framework directly supports the acceptance criteria for Epic #82 and its child User Stories (#81-#88):

#### Linked User Stories

| ID | Title | QA Phase Alignment |
|:---|:------|:-----|
| **#81** | Refactor calculator with Angular UI frontend while maintaining console project | Phases 8, 9, 16 |
| **#83** | Perform multiple calculations in sequence without restarting | Phase 14.1 (User Workflows) |
| **#84** | Build well-structured and tested code following .NET best practices | Phases 1, 6, 11 |
| **#85** | Handle invalid inputs gracefully without crashing | Phases 9.1-9.3 (Error Recovery) |
| **#86** | Verify calculator functions with comprehensive unit and E2E tests | Phases 2, 7, 14 (E2E Testing) |
| **#87** | Perform basic arithmetic calculations with two operands | Phase 2, 7.3 (Manual Verification) |
| **#88** | Receive clear prompts and feedback during calculation | Phases 8, 16 (Documentation) |

### Acceptance Criteria Mapping

#### Story #87: Perform basic arithmetic calculations with two operands

**QA Validation:**
```markdown
✅ Phase 2: Test Coverage & Validation
   - All 51+ tests pass including arithmetic operation tests
   - Test data includes all 6 operations: +, -, *, /, %, ^

✅ Phase 7.3: Manual Verification
   - Calculator correctly computes: 5 + 3 = 8
   - Calculator correctly computes: 10 - 4 = 6
   - Calculator correctly computes: 6 * 7 = 42
   - Calculator correctly computes: 20 / 4 = 5
   - Calculator correctly computes: 17 % 5 = 2
   - Calculator correctly computes: 2 ^ 8 = 256

✅ Phase 11: Code Maintainability
   - Calculator.cs has public static methods for each operation
   - Calculate() dispatcher method routes operations correctly
```

**Success Criteria:**
- ✅ All arithmetic operations produce correct results
- ✅ Results accurate to 10 decimal places (floating-point precision)
- ✅ Large numbers (> 1,000,000) computed without overflow
- ✅ Negative numbers handled correctly in all operations

#### Story #83: Perform multiple calculations in sequence without restarting

**QA Validation:**
```markdown
✅ Phase 14.1: User Workflow Testing
   - Test continuous calculation workflow
   - Verify state consistency across multiple operations
   - Confirm no memory degradation after 100+ calculations

✅ Phase 9.3: State Management & Recovery
   - Application in known state after each operation
   - No dangling resources between calculations
   - Variables reset for next operation
   - Error in one calculation doesn't affect next
```

**Success Criteria:**
- ✅ User can perform 100+ consecutive calculations
- ✅ Each result displayed correctly without interference
- ✅ Memory usage remains stable across session
- ✅ Performance doesn't degrade with extended use
- ✅ No memory leaks detected

#### Story #84: Build well-structured and tested code following .NET best practices

**QA Validation:**
```markdown
✅ Phase 1: Code Quality Analysis
   - Static analysis: Zero C# code style violations
   - Naming conventions: 100% compliance with PascalCase/camelCase
   - All public methods documented with XML comments

✅ Phase 6: Project Standards Compliance
   - Projects target net8.0 framework
   - ImplicitUsings enabled
   - Nullable reference types enabled
   - Project structure matches PRD specification

✅ Phase 11: Code Maintainability & Technical Debt
   - Cyclomatic complexity < 10 per method
   - Maintainability Index > 80
   - Code duplication < 3%
   - No untracked TODO/FIXME comments

✅ Phase 3: Security & Vulnerability Assessment
   - Zero NuGet package CVEs
   - No security anti-patterns detected
   - All inputs properly validated
   - Exception handling doesn't leak sensitive data
```

**Success Criteria:**
- ✅ Code adheres to Microsoft C# Coding Conventions
- ✅ All public methods documented (100% coverage)
- ✅ No hardcoded magic numbers or sensitive values
- ✅ Proper exception handling throughout
- ✅ Null safety enabled and validated

#### Story #85: Handle invalid inputs gracefully without crashing

**QA Validation:**
```markdown
✅ Phase 9: Error Recovery & Resilience
   - Division by zero throws ArgumentException with clear message
   - Invalid operators caught and reported
   - Null/empty inputs handled without crashes
   - Whitespace properly trimmed from inputs
   - Extreme values (MaxValue, MinValue) managed correctly

✅ Phase 9.2: Input Sanitization & Validation
   - All inputs validated before processing
   - No buffer overflow vulnerabilities
   - Special characters rejected gracefully
   - Leading zeros handled correctly
   - Scientific notation recognized or rejected appropriately

✅ Phase 9.3: State Management & Recovery
   - Application returns to stable state after error
   - User can retry after invalid input
   - No partial results or corrupted state
   - Application continues running after error
```

**Success Criteria:**
- ✅ No unhandled exceptions reach user
- ✅ Error messages describe what went wrong
- ✅ User can recover and retry after any error
- ✅ Invalid operator "++" rejected with clear message
- ✅ Division/modulo by zero handled with meaningful error
- ✅ Application never crashes or hangs

#### Story #86: Verify calculator functions with comprehensive unit and E2E tests

**QA Validation:**
```markdown
✅ Phase 2: Test Coverage & Validation
   - 51+ tests implemented and passing
   - Code coverage ≥ 80%
   - All public methods covered by tests
   - All error paths tested
   - Edge cases covered (boundary values, null, zero)

✅ Phase 2.5: QA Engineer Capabilities
   - Automated test case generation for all code paths
   - Edge case analysis identifies untested scenarios
   - Test data generation expands coverage
   - Regression tests prevent issue recurrence
   - Code coverage gaps eliminated (100% on public methods)

✅ Phase 7: Integration & End-to-End Testing
   - Solution builds without errors
   - Full test suite execution succeeds
   - No flaky or intermittent failures
   - Manual verification of all operations
```

**Success Criteria:**
- ✅ All 51+ tests pass (0 failures)
- ✅ 80%+ code coverage achieved
- ✅ Unit tests cover all operations
- ✅ Edge cases tested (zero, negative, large numbers)
- ✅ Error scenarios tested (division by zero, invalid operators)
- ✅ E2E tests verify complete workflows
- ✅ Test data integrity verified

#### Story #88: Receive clear prompts and feedback during calculation

**QA Validation:**
```markdown
✅ Phase 8: User Experience & Accessibility
   - Console application prompts clear and unambiguous
   - Operator guidance displayed and helpful
   - Error messages user-friendly and actionable
   - Result formatting readable and consistent
   - Console screen clears properly between operations

✅ Phase 8.2: User Input Experience
   - Valid inputs processed correctly
   - Invalid number input shows clear error
   - Empty operator shows error message
   - Invalid operator rejected gracefully
   - User can retry after any error

✅ Phase 16: Documentation Completeness
   - README includes quick start guide
   - Usage examples provided for each operation
   - Troubleshooting guide available
   - Error messages documented
```

**Success Criteria:**
- ✅ All prompts clearly indicate what input is expected
- ✅ Operator list displayed to user (+, -, *, /, %, ^)
- ✅ Calculation result shown immediately after input
- ✅ Error messages identify the problem specifically
- ✅ Help/guidance available without errors
- ✅ No cryptic error codes shown to user

#### Story #81: Refactor calculator with Angular UI frontend while maintaining console project

**QA Validation (Future):**
```markdown
✅ Phase 8: User Experience & Accessibility
   - Angular UI provides same functionality as console
   - All operations available in both interfaces
   - Error handling consistent between UI and console
   - Input validation identical in both implementations

✅ Phase 7: Integration & End-to-End Testing
   - Console project continues working independently
   - API/library layer can be called from both UIs
   - Shared Calculator logic tested in both contexts
   - No regression in existing console functionality

✅ Phase 16: Documentation Completeness
   - Documentation covers both UI options
   - API documentation clear for both consumers
   - Migration guide from console to web UI provided
```

---

## Quality Assurance Execution Steps

**17 Comprehensive Phases covering 50+ distinct quality checks, aligned with ADO work items:**

1. **Code Quality Analysis** – Static analysis, complexity, naming conventions
2. **Test Coverage & Validation** – Test execution, effectiveness, data integrity
3. **QA Engineer Capabilities** – Automated tests, edge cases, data generation (8 QA-specific subsections)
4. **Security & Vulnerability Assessment** – Packages, code security, dependencies
5. **Performance & Optimization** – Build time, test speed, runtime
6. **Documentation & Compliance** – Code docs, README, commit messages
7. **Project Standards Compliance** – Framework, structure, .csproj validation
8. **Integration & End-to-End Testing** – Full builds, complete test suite, manual verification
9. **User Experience & Accessibility** – Usability, input experience, accessibility
10. **Error Recovery & Resilience** – Error handling, input sanitization, state management
11. **Logging, Diagnostics & Observability** – Error logging, performance metrics
12. **Code Maintainability & Technical Debt** – Maintainability index, technical debt identification
13. **Deployment Readiness & Distribution** – Single executable, environment independence
14. **Build Environment Consistency** – Reproducible builds, environment requirements
15. **Real-World Scenario Testing** – User workflows, load/stress testing
16. **Code Review & Team Standards** – Review process, branch protection
17. **Documentation Completeness** – User guides, API docs, architecture

---

### Phase 3: Security & Vulnerability Assessment

#### 1.1 Static Code Analysis
Execute the following checks:

```powershell
# Run .NET code analyzers
dotnet build /p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true /p:TreatWarningsAsErrors=true

# Check for code quality issues
dotnet format --verify-no-changes --verbosity quiet
```

**Validation Criteria:**
- ✅ Zero C# code style violations
- ✅ No compiler errors (warnings acceptable only for framework/SDK)
- ✅ Code adheres to Microsoft C# Coding Conventions
- ✅ All public methods properly documented with XML comments

#### 1.2 Cyclomatic Complexity & Code Metrics
Verify code maintainability:

```powershell
# Analyze project structure
$projects = @(
    "calculator/calculator.csproj",
    "calculator.tests/calculator.tests.csproj"
)

foreach ($project in $projects) {
    Write-Host "Analyzing $project..."
    # Check method complexity (should be < 10 for public methods)
    # Check class cohesion (should be > 0.7)
    # Check code duplication (should be < 3%)
}
```

**Validation Criteria:**
- ✅ All public methods have cyclomatic complexity < 10
- ✅ No duplicate code blocks > 3 lines
- ✅ Class cohesion score > 0.7
- ✅ Average method lines < 20

#### 1.3 Naming Conventions & Consistency
Verify compliance with .NET naming standards:

```csharp
// Expected Patterns:
// ✅ Classes: PascalCase (Calculator, CalculatorTest)
// ✅ Methods: PascalCase (Add, Subtract, Calculate)
// ✅ Properties: PascalCase
// ✅ Private fields: _camelCase (_fieldName)
// ✅ Local variables: camelCase (result, operand)
// ✅ Constants: UPPER_SNAKE_CASE or PascalCase
```

**Validation Criteria:**
- ✅ 100% compliance with naming conventions
- ✅ No inconsistent naming patterns
- ✅ All acronyms capitalized correctly

---

### Phase 2: Test Coverage & Validation

#### 2.1 Comprehensive Test Execution
Run all unit tests with coverage reporting:

```powershell
cd calculator-xunit-testing

# Run tests with detailed output
dotnet test --configuration Release --logger "console;verbosity=detailed" --collect:"XPlat Code Coverage"

# Generate coverage report
reportgenerator -reports:**/coverage.cobertura.xml -reporttypes:Html -targetdir:./coverage-report
```

**Validation Criteria:**
- ✅ All 51+ tests pass (0 failures)
- ✅ Code coverage ≥ 80%
- ✅ All public methods covered by tests
- ✅ All error paths tested
- ✅ Edge cases covered (boundary values, null inputs, zero values)

#### 2.2 Test Quality Assessment
Validate test effectiveness:

```powershell
# Test naming convention check
$testFiles = Get-ChildItem -Path "calculator.tests" -Filter "*.cs" -Recurse
foreach ($file in $testFiles) {
    # Verify: MethodName_Scenario_ExpectedResult pattern
    # Verify: [Fact] for single scenarios, [Theory] for multiple
    # Verify: Arrange-Act-Assert structure
    # Verify: Clear test descriptions
}
```

**Validation Criteria:**
- ✅ All tests follow `MethodName_Scenario_ExpectedResult` naming
- ✅ Tests organized in AAA pattern (Arrange-Act-Assert)
- ✅ No hardcoded test data in test methods (use CSV/InlineData)
- ✅ All assertions use appropriate Assert methods
- ✅ No test interdependencies

#### 2.3 Test Data Integrity
Validate CSV test data:

```powershell
# Verify CalculatorTestData.csv
$csvPath = "calculator.tests/TestData/CalculatorTestData.csv"
$csvData = Import-Csv $csvPath

# Validation checks:
# - All required columns present (firstNumber, secondNumber, operator, expectedResult, testDescription)
# - No empty cells in critical columns
# - Operator values valid (+, -, *, /, %, ^)
# - expectedResult is numeric where applicable
# - Test descriptions are meaningful
# - No duplicate test cases
```

**Validation Criteria:**
- ✅ CSV file well-formed with proper headers
- ✅ All 31 test cases valid
- ✅ No empty or malformed data rows
- ✅ Test data covers all operations
- ✅ Edge cases included (zero, negative numbers, decimals)

---

### Phase 2.5: QA Engineer Capabilities (Persona-Specific)

#### 2.5.1 Automated Test Case Generation
Identify and generate missing test cases:

```powershell
# Analyze existing tests and identify gaps
$testMethods = @(
    "Add_TwoPositiveNumbers_ReturnsSum",
    "Add_NegativeNumbers_ReturnsSum",
    "Add_ZeroPlusNumber_ReturnsNumber",
    "Subtract_PositiveNumbers_ReturnsDifference",
    "Multiply_PositiveNumbers_ReturnsProduct",
    "Divide_PositiveNumbers_ReturnsQuotient",
    "Modulo_PositiveNumbers_ReturnsRemainder",
    "Power_BaseAndExponent_ReturnsResult"
)

# For each operation, verify:
# - Positive cases tested
# - Negative cases tested
# - Zero cases tested
# - Overflow/precision cases tested
# - Invalid input cases tested
```

**Generated Test Categories:**
- ✅ Happy path tests (normal valid inputs)
- ✅ Boundary value tests (0, min, max, near-limits)
- ✅ Negative input tests (negative numbers)
- ✅ Precision tests (floating-point accuracy)
- ✅ Error case tests (division by zero, invalid operators)

#### 2.5.2 Edge Case Suggestions & Analysis
Identify untested edge cases:

```csharp
// Edge cases to test:
// 1. Zero operands: 0 + X, X - 0, 0 * X, 0 / X, X % 0, 0 ^ X
// 2. Negative numbers: -X + Y, X - (-Y), -X * -Y, -X / Y
// 3. Very large numbers: 999999999 + 999999999
// 4. Very small (precision): 0.0000001 + 0.0000002
// 5. Fractional results: 5 / 2 = 2.5
// 6. Special floats: double.MaxValue, double.MinValue, double.Epsilon
// 7. Invalid operators: &, |, !, ~, @, #
// 8. Whitespace handling: " + ", "+", " +"
// 9. Case sensitivity: "ADD", "add", "Add"
// 10. Empty/null inputs
```

**Validation Criteria:**
- ✅ All boundary values tested
- ✅ All error conditions identified
- ✅ Precision requirements validated
- ✅ Type coercion tested
- ✅ Operator symbol variations covered

#### 2.5.3 Test Data Generation & Expansion
Generate comprehensive test datasets:

```powershell
# Generate synthetic test data
function Generate-TestData {
    $operations = @('+', '-', '*', '/', '%', '^')
    $categories = @(
        @{name="Basic"; nums=@(1, 2, 5, 10)},
        @{name="Large"; nums=@(1000000, 9999999, [double]::MaxValue)},
        @{name="Small"; nums=@(0.0001, 0.000001, [double]::MinValue)},
        @{name="Negative"; nums=@(-1, -5, -999)},
        @{name="Zero"; nums=@(0)},
        @{name="Mixed"; nums=@(-5, 0, 5, 3.14, 2.71)}
    )
    
    foreach ($category in $categories) {
        foreach ($op in $operations) {
            foreach ($a in $category.nums) {
                foreach ($b in $category.nums) {
                    # Generate expected result
                    # Output to CSV
                }
            }
        }
    }
}
```

**Generated Datasets:**
- ✅ Basic arithmetic combinations
- ✅ Boundary value sets
- ✅ Error condition sets
- ✅ Precision/floating-point sets
- ✅ Performance stress test sets

#### 2.5.4 Bug Reproduction Assistance
Create test cases to reproduce known issues:

```powershell
# Bug reproduction test template
<#
.SYNOPSIS
Reproduces Bug #XXX: [Bug Description]

.DESCRIPTION
Steps to reproduce the issue

.PARAMETER operand1
First operand value

.PARAMETER operand2
Second operand value

.PARAMETER operator
Operation to perform

.EXAMPLE
Test-BugRepro -operand1 5 -operand2 0 -operator "/"
#>

function Test-BugRepro {
    param(
        [double]$operand1,
        [double]$operand2,
        [string]$operator
    )
    
    # Execute operation
    # Verify actual behavior matches bug report
    # Document findings
}
```

**Validation Criteria:**
- ✅ All reported bugs have reproducible tests
- ✅ Root cause identified through testing
- ✅ Fix verified with test execution
- ✅ Regression prevention tests written

#### 2.5.5 Mock Data & Stubbing Patterns
Create reusable test fixtures:

```csharp
// Mock data fixtures
public static class CalculatorTestFixtures
{
    /// <summary>Valid test operand pairs with expected results</summary>
    public static IEnumerable<object[]> ValidOperandPairs => new[]
    {
        new object[] { 5.0, 3.0, "+", 8.0 },
        new object[] { 10.0, 4.0, "-", 6.0 },
        new object[] { 6.0, 7.0, "*", 42.0 },
        new object[] { 20.0, 4.0, "/", 5.0 }
    };

    /// <summary>Invalid test cases for error handling</summary>
    public static IEnumerable<object[]> InvalidOperatorPairs => new[]
    {
        new object[] { 5.0, 3.0, "&" },
        new object[] { 5.0, 3.0, "|" },
        new object[] { 5.0, 3.0, "^" }  // Note: ^ is power, not XOR
    };

    /// <summary>Boundary value test fixtures</summary>
    public static IEnumerable<object[]> BoundaryValuePairs => new[]
    {
        new object[] { 0.0, 5.0, "+", 5.0 },
        new object[] { 5.0, 0.0, "-", 5.0 },
        new object[] { 0.0, 0.0, "*", 0.0 }
    };
}
```

**Fixture Categories:**
- ✅ Valid data fixtures
- ✅ Invalid/error data fixtures
- ✅ Boundary value fixtures
- ✅ Performance test fixtures
- ✅ Regression test fixtures

#### 2.5.6 Regression Test Writing
Create tests for previously fixed issues:

```csharp
/// <summary>
/// Regression tests for previously discovered and fixed issues.
/// These tests ensure fixes remain effective through future refactoring.
/// </summary>
public class CalculatorRegressionTests
{
    [Fact]
    public void Regression_DivisionByZero_ThrowsExceptionWithClearMessage()
    {
        // Regression: Issue #42 - Division by zero should throw ArgumentException, not NullReferenceException
        var ex = Assert.Throws<ArgumentException>(() => Calculator.Divide(5, 0));
        Assert.Contains("divide by zero", ex.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Theory]
    [InlineData(2, 10, 1024)]
    [InlineData(10, 2, 100)]
    public void Regression_PowerCalculation_HighPrecision(double baseNum, double exponent, double expected)
    {
        // Regression: Issue #38 - Power calculations were losing precision
        var result = Calculator.Power(baseNum, exponent);
        Assert.Equal(expected, result, precision: 10);
    }

    [Theory]
    [InlineData(-5, 3, -2)]
    [InlineData(-10, -3, -1)]
    public void Regression_ModuloNegativeNumbers_CorrectResult(double a, double b, double expected)
    {
        // Regression: Issue #45 - Modulo with negative numbers returned incorrect sign
        var result = Calculator.Modulo(a, b);
        Assert.Equal(expected, result);
    }
}
```

**Regression Test Coverage:**
- ✅ All fixed bugs have regression tests
- ✅ Tests document issue numbers and descriptions
- ✅ Tests verify exact fix implementation
- ✅ Tests prevent issue recurrence

#### 2.5.7 Code Coverage Improvement Analysis
Identify and eliminate coverage gaps:

```powershell
# Analyze coverage report
$coverageFile = "./test-results/coverage.cobertura.xml"
[xml]$coverage = Get-Content $coverageFile

# Identify uncovered lines
$uncoveredLines = $coverage.coverage.packages.package.classes.class.lines.line | 
    Where-Object { $_.hits -eq 0 }

# For each uncovered line, determine:
# - Why it's uncovered (dead code? error path? edge case?)
# - Should it be tested or removed?
# - Generate appropriate test cases

foreach ($line in $uncoveredLines) {
    Write-Host "Uncovered line $($line.number): $($line.text)"
    # Generate test to cover or mark as intentionally uncovered
}
```

**Coverage Improvement Actions:**
- ✅ Identify uncovered code paths
- ✅ Generate tests for untested code
- ✅ Remove dead code or mark as intentional
- ✅ Target 80%+ coverage
- ✅ 100% coverage on public methods

#### 2.5.8 Automation Scripting & Framework Integration
Create reusable test automation scripts:

```powershell
# QA Automation Framework - Calculator Testing Suite
<#
.SYNOPSIS
Comprehensive test automation framework for calculator QA

.DESCRIPTION
Provides automated testing capabilities for:
- Batch test execution
- Result reporting
- Performance tracking
- Regression detection
- Coverage measurement
#>

function Invoke-CalculatorQATests {
    [CmdletBinding()]
    param(
        [ValidateSet('Quick', 'Full', 'Regression', 'Performance')]
        [string]$TestSuite = 'Full',
        
        [switch]$GenerateCoverageReport,
        [switch]$CompareWithBaseline
    )

    $testPath = "calculator.tests/bin/Release/net8.0/calculator.tests.dll"
    
    # Execute tests based on suite type
    switch ($TestSuite) {
        'Quick' {
            dotnet test --filter "Category=Quick" --configuration Release
        }
        'Full' {
            dotnet test --configuration Release --collect:"XPlat Code Coverage"
        }
        'Regression' {
            dotnet test --filter "Category=Regression" --configuration Release
        }
        'Performance' {
            dotnet test --filter "Category=Performance" --configuration Release
        }
    }

    if ($GenerateCoverageReport) {
        reportgenerator -reports:**/coverage.cobertura.xml -reporttypes:Html -targetdir:./coverage-report
    }

    if ($CompareWithBaseline) {
        # Compare with previous coverage baseline
        # Alert on coverage regressions
    }
}

# Usage:
# Invoke-CalculatorQATests -TestSuite Full -GenerateCoverageReport -CompareWithBaseline
```

**Automation Capabilities:**
- ✅ Test suite orchestration
- ✅ Results aggregation and reporting
- ✅ Coverage tracking and trending
- ✅ Performance regression detection
- ✅ Automated quality gates enforcement

---

### Phase 8: User Experience & Accessibility

#### 8.1 Console Application Usability
Validate user experience and UI/UX quality:

```powershell
# Verify console application usability
$checks = @(
    "Clear menu/prompt structure",
    "Helpful operator guide displayed",
    "Error messages are user-friendly and actionable",
    "Input prompts are clear and unambiguous",
    "Results displayed with proper formatting",
    "Help/exit options available",
    "No console artifacts or formatting issues"
)

foreach ($check in $checks) {
    Write-Host "Validating: $check"
}
```

**Validation Criteria:**
- ✅ All prompts clear and unambiguous
- ✅ Operator guidance displayed (supports +, -, *, /, %, ^)
- ✅ Error messages are helpful (not cryptic)
- ✅ Result formatting is readable and consistent
- ✅ Console screen clears properly between operations
- ✅ No unterminated loops or hanging states
- ✅ Application exits gracefully

#### 8.2 User Input Experience
Test user interaction scenarios:

```csharp
// Test scenarios:
// 1. Happy path: Valid inputs → Expected output
// 2. Invalid number input: "abc" → Clear error, retry
// 3. Empty operator: "" → Error message, retry
// 4. Invalid operator: "&" → Error message, retry
// 5. Division by zero: "5 / 0" → Handled gracefully
// 6. Very large numbers: "999999999 + 999999999" → Computed correctly
// 7. Rapid input: Continuous calculations work correctly
// 8. Exit scenario: User can exit cleanly
```

**Validation Criteria:**
- ✅ User can recover from invalid input
- ✅ No exception stack traces visible to users
- ✅ Clear guidance on valid operators
- ✅ Graceful error handling for edge cases
- ✅ User can repeat calculations or exit

#### 8.3 Accessibility Considerations
Ensure console application accessibility:

```powershell
# Accessibility checks
$accessibilityChecks = @(
    "No color-dependent information (suitable for color-blind users)",
    "All information conveyed through text (not just colors/symbols)",
    "Readable font sizes and contrast",
    "Screen reader compatible (text-based prompts)",
    "No flickering or rapidly changing content",
    "Clear keyboard navigation (no mouse required)",
    "All input validated and echoed back"
)
```

**Validation Criteria:**
- ✅ Console text is readable and high-contrast
- ✅ No color-only differentiation of information
- ✅ All prompts are text-based (screen reader friendly)
- ✅ No rapid flashing or visual tricks
- ✅ Keyboard-only operation supported
- ✅ Error messages audible (text output)

---

### Phase 9: Error Recovery & Resilience

#### 9.1 Graceful Error Handling
Test error recovery and resilience:

```csharp
// Error handling validation:
// 1. Division by zero: ArgumentException with clear message
// 2. Invalid operator: ArgumentException identifying the operator
// 3. Null/empty input: Handled without crashes
// 4. Whitespace in operators: Trimmed and validated
// 5. Case-sensitivity: Operators validated (+ works, not ++  or +x)
// 6. Extreme values: Math.MaxValue, Math.MinValue handled
// 7. NaN/Infinity: Detected and reported
```

**Validation Criteria:**
- ✅ No unhandled exceptions reach user
- ✅ Error messages include what went wrong and why
- ✅ All error paths return to stable state
- ✅ No partial results or corrupted state
- ✅ User can retry after error
- ✅ Application continues running after error

#### 9.2 Input Sanitization & Validation
Verify robust input handling:

```powershell
# Input sanitization tests
$sanitizationTests = @(
    "Whitespace trimming: '  5  ' → 5",
    "Empty input handling: '' → Error",
    "Null input handling: null → Error",
    "Special characters: '5!', '5#' → Error",
    "Leading zeros: '007' → 7",
    "Scientific notation: '1e5' → 100000",
    "Negative numbers: '-5' → -5",
    "Decimal precision: '3.14159' → Preserved"
)
```

**Validation Criteria:**
- ✅ All inputs validated before processing
- ✅ Whitespace properly handled
- ✅ No buffer overflow vulnerabilities
- ✅ No injection attack vectors
- ✅ Edge case numbers handled (very small, very large)
- ✅ Precision maintained for valid inputs

#### 9.3 State Management & Recovery
Ensure consistent application state:

```powershell
# State validation
# After each operation:
#   1. Application in known state
#   2. No dangling resources
#   3. No memory leaks
#   4. Variables reset for next operation
#   5. Error state doesn't affect future calculations
```

**Validation Criteria:**
- ✅ State consistent after each operation
- ✅ No resource leaks (files, memory, connections)
- ✅ Error in one calculation doesn't affect next
- ✅ Multiple consecutive operations work correctly
- ✅ Application responsive after errors

---

### Phase 10: Logging, Diagnostics & Observability

#### 10.1 Error Logging & Diagnostics
Implement diagnostic capabilities:

```csharp
// Logging considerations:
// 1. Operation logging: Track what calculations were performed
// 2. Error logging: Capture error details (input, operation, reason)
// 3. Performance logging: Track calculation times
// 4. Debug output: Optional verbose mode for troubleshooting
// 5. Event tracing: Correlate user actions with system events
```

**Validation Criteria:**
- ✅ Errors logged with sufficient context
- ✅ No sensitive data in logs
- ✅ Log format consistent and parseable
- ✅ Log levels appropriate (Info, Warning, Error)
- ✅ Debug mode available for troubleshooting
- ✅ Logs don't impact performance

#### 10.2 Performance Metrics & Monitoring
Track application performance:

```powershell
# Performance monitoring
$performanceMetrics = @(
    "Calculation time: < 100ms per operation",
    "Memory usage: Stable, no growth",
    "Startup time: < 500ms",
    "Response time: < 200ms to user input",
    "No CPU spikes during calculation"
)

# Optional: Implement performance logging
```

**Validation Criteria:**
- ✅ All operations complete within acceptable time
- ✅ Memory usage remains stable
- ✅ No memory leaks on repeated operations
- ✅ CPU usage reasonable (< 50% for single calc)
- ✅ Performance degrades gracefully under load

---

### Phase 11: Code Maintainability & Technical Debt

#### 11.1 Maintainability Index Analysis
Calculate code health metrics:

```powershell
# Maintainability metrics
$maintainabilityChecks = @{
    "Cyclomatic Complexity" = "< 10 per method"
    "Maintainability Index" = "> 80 (high maintainability)"
    "Lines of Code per Method" = "< 25 lines"
    "Code Duplication" = "< 3%"
    "Comment Ratio" = "15-20% of code"
    "Class Cohesion" = "> 0.7"
}

# Tools: 
# - Microsoft.CodeAnalysis.CSharp
# - Roslyn analyzers
# - VS Code extensions
```

**Validation Criteria:**
- ✅ Maintainability Index > 80
- ✅ No methods with complexity > 10
- ✅ Average method < 20 lines
- ✅ Code duplication < 3%
- ✅ Comments present but not excessive
- ✅ Classes have high cohesion

#### 11.2 Technical Debt Identification
Identify and track technical debt:

```csharp
// Identify technical debt:
// 1. TODO/FIXME comments → Document with issue numbers
// 2. Magic numbers → Extract to named constants
// 3. Unused variables/imports → Remove
// 4. Deprecated APIs → Plan migration
// 5. Code smell indicators → Flag for refactoring
// 6. Performance optimization opportunities → Measure first
```

**Validation Criteria:**
- ✅ No untracked TODO/FIXME comments
- ✅ All magic numbers documented or extracted
- ✅ No unused code
- ✅ No deprecated APIs used
- ✅ Refactoring backlog tracked
- ✅ Technical debt quantified

---

### Phase 12: Deployment Readiness & Distribution

#### 12.1 Single Executable & Deployment
Verify deployment readiness:

```powershell
# Deployment readiness checks
$deploymentChecks = @(
    "Can build as single executable: dotnet publish -c Release",
    "No external dependencies required (self-contained)",
    "Works on target platforms without install",
    "Executable has proper metadata (version, product info)",
    "File size is reasonable (< 100MB for console app)",
    "No hardcoded paths or environment variables"
)
```

**Validation Criteria:**
- ✅ Can publish as self-contained single executable
- ✅ No external DLL dependencies
- ✅ Works on Windows, Linux, macOS (if net8.0)
- ✅ Assembly info set (version, company, copyright)
- ✅ Reasonable executable size
- ✅ No configuration files required

#### 12.2 Environment Independence
Ensure portability across environments:

```powershell
# Environment independence tests
# 1. Works from any directory
# 2. Works with or without .NET SDK installed
# 3. Works on machines without VS Code/Visual Studio
# 4. Works from CI/CD pipeline
# 5. Works with restricted permissions
```

**Validation Criteria:**
- ✅ No hardcoded local paths
- ✅ No registry dependencies
- ✅ No shell-specific features
- ✅ Cross-platform compatible
- ✅ Works in minimal environments

---

### Phase 13: Build Environment Consistency

#### 13.1 Reproducible Builds
Ensure builds are consistent across environments:

```powershell
# Reproducible build validation
<#
.DESCRIPTION
Verify builds produce identical binaries across different machines/times

Tests:
1. Build on Windows, Linux, macOS → Same hash
2. Clean rebuild → Same binary
3. Different build times → Same hash
4. Different build machines → Same hash
#>

$build1 = dotnet build --configuration Release
$hash1 = (Get-FileHash "calculator/bin/Release/net8.0/calculator.dll").Hash

# ... rebuild and verify hash matches
```

**Validation Criteria:**
- ✅ Builds are deterministic
- ✅ Same source code produces identical binary
- ✅ No timestamp differences in assemblies
- ✅ Reproducible across build environments
- ✅ No build artifacts in binary

#### 13.2 Build Environment Requirements
Document and validate build environment:

```powershell
# Build environment requirements
$buildRequirements = @{
    ".NET SDK" = "net8.0"
    "C# Language" = "11.0"
    "MSBuild" = "Included with SDK"
    "Git" = "Optional (for version info)"
    "PowerShell" = "5.0+"
}

# Validate environment has all requirements
foreach ($requirement in $buildRequirements.GetEnumerator()) {
    Write-Host "Checking: $($requirement.Name) = $($requirement.Value)"
}
```

**Validation Criteria:**
- ✅ .NET 8.0 SDK available
- ✅ C# 11+ language support
- ✅ All tools in PATH
- ✅ Compatible with CI/CD agents
- ✅ Setup documentation provided

---

### Phase 14: Real-World Scenario Testing

#### 14.1 User Workflow Testing
Test complete user workflows:

```powershell
# Test complete workflows
$workflows = @(
    @{
        name = "Basic Calculation Workflow"
        steps = @("Add", "Multiply", "Divide")
        expectedPasses = 3
    },
    @{
        name = "Error Recovery Workflow"
        steps = @("InvalidInput", "ValidCalc", "DivideByZero", "ValidCalc")
        expectedPasses = 2
    },
    @{
        name = "Extended Session"
        steps = @("Calc1", "Calc2", "Calc3", ... "Calc100")
        expectedPasses = 100
    }
)
```

**Validation Criteria:**
- ✅ Multi-step workflows complete successfully
- ✅ User can recover from errors and continue
- ✅ Long sessions don't degrade performance
- ✅ State remains consistent across many operations
- ✅ Resource usage doesn't grow unbounded

#### 14.2 Load & Stress Testing
Test application under load:

```powershell
# Stress testing
function Test-CalculatorLoad {
    param([int]$iterations = 10000)
    
    $stopwatch = [System.Diagnostics.Stopwatch]::StartNew()
    
    for ($i = 0; $i -lt $iterations; $i++) {
        # Rapid calculations: addition, multiplication, division
        $calc = [Calculator]::Add(5, 3)
        $calc = [Calculator]::Multiply(10, 20)
        $calc = [Calculator]::Divide(100, 5)
    }
    
    $stopwatch.Stop()
    Write-Host "Completed $iterations iterations in $($stopwatch.ElapsedMilliseconds)ms"
}
```

**Validation Criteria:**
- ✅ Handles 10,000+ consecutive operations
- ✅ No performance degradation under load
- ✅ No memory leaks during extended use
- ✅ Consistent timing across iterations
- ✅ Graceful handling at resource limits

---

### Phase 15: Code Review & Team Standards

#### 15.1 Code Review Checklist
Ensure all changes go through proper review:

```markdown
## Code Review Requirements

- [ ] Code follows naming conventions (PascalCase/camelCase)
- [ ] All public methods have XML documentation
- [ ] Tests added for new functionality
- [ ] No console.WriteLine() without reason (use structured logging)
- [ ] Error messages are user-friendly
- [ ] No hardcoded values (use constants)
- [ ] Security review completed
- [ ] Performance implications considered
- [ ] Backward compatibility maintained
- [ ] Commit messages follow topic(subtopic): activity format
```

**Validation Criteria:**
- ✅ All changes reviewed by at least one team member
- ✅ Code review checklist completed
- ✅ No approvals from code authors (peer review only)
- ✅ Issues resolved before merge
- ✅ CI/CD checks pass

#### 15.2 Git Workflow & Branch Protection
Enforce branching and merge policies:

```powershell
# Branch protection rules
$branchProtection = @{
    "Main Branch" = @{
        "RequiresPullRequest" = $true
        "RequiresCodeReview" = $true
        "MinimumApprovals" = 1
        "DismissApprovals" = $false
        "RequiresStatusChecks" = @("Build", "Tests", "CodeQuality")
        "AllowForcePush" = $false
    }
}
```

**Validation Criteria:**
- ✅ Direct commits to main blocked
- ✅ Pull requests required for changes
- ✅ Code review required before merge
- ✅ CI/CD checks must pass
- ✅ Status checks required
- ✅ Force push disabled

---

### Phase 16: Documentation Completeness

#### 16.1 User & Developer Documentation
Verify documentation quality:

```markdown
## Documentation Checklist

### User Documentation
- [ ] Quick start guide (< 5 minutes to first calculation)
- [ ] Usage examples for each operation
- [ ] Troubleshooting guide
- [ ] Limitations and known issues documented
- [ ] FAQ section

### Developer Documentation
- [ ] Architecture overview
- [ ] Code structure explanation
- [ ] How to add new operations (extension guide)
- [ ] Setup instructions for developers
- [ ] Contribution guidelines
- [ ] Testing guide

### API Documentation
- [ ] All public classes documented
- [ ] All public methods documented
- [ ] All parameters documented
- [ ] Return values documented
- [ ] Exceptions documented
```

**Validation Criteria:**
- ✅ README.md is comprehensive and current
- ✅ API documentation 100% coverage
- ✅ Examples are tested and work
- ✅ No broken links in documentation
- ✅ Documentation reflects current version

#### 16.2 Architecture & Design Documentation
Document system design:

```markdown
## Architecture Documentation

- [ ] System architecture diagram
- [ ] Component responsibilities defined
- [ ] Data flow explained
- [ ] Design patterns documented
- [ ] Performance considerations noted
- [ ] Security architecture explained
- [ ] Extension points identified
- [ ] Future enhancements outlined
```

**Validation Criteria:**
- ✅ Architecture is well-documented
- ✅ Design decisions explained
- ✅ Future maintainers can understand system
- ✅ Extension points are clear

---



#### 3.1 NuGet Package Vulnerability Scan
Check for known vulnerabilities:

```powershell
# Scan for vulnerabilities
dotnet package search --exact

# Check specific packages
$packages = @(
    "xunit|2.6.2",
    "xunit.runner.visualstudio|2.5.1",
    "Microsoft.NET.Test.Sdk|17.5.0",
    "coverlet.collector|6.0.0"
)

foreach ($package in $packages) {
    Write-Host "Checking $package for CVEs..."
}
```

**Validation Criteria:**
- ✅ No high/critical CVEs in dependencies
- ✅ All packages from trusted sources (NuGet.org)
- ✅ No abandoned/unmaintained packages
- ✅ Package licenses compatible with project

#### 3.2 Code Security Review
Check for security anti-patterns:

```csharp
// ✅ Validation: No hardcoded credentials, API keys, or secrets
// ✅ No SQL injection vulnerabilities (N/A for console app)
// ✅ Proper exception handling (no sensitive info in messages)
// ✅ Input validation on all user inputs
// ✅ No unsafe code blocks
// ✅ Proper disposal of resources
// ✅ No use of deprecated/dangerous APIs
```

**Validation Criteria:**
- ✅ Zero security anti-patterns detected
- ✅ All user inputs validated
- ✅ Exception handling doesn't leak sensitive info
- ✅ No hardcoded secrets in code
- ✅ Null safety enabled (`<Nullable>enable</Nullable>`)

#### 3.3 Dependency Chain Security
Validate transitive dependencies:

```powershell
# List all dependencies including transitive
dotnet list package --include-transitive

# Check for conflicts or known issues
# - No duplicate packages with different versions
# - No known CVEs in transitive dependencies
# - All dependencies have known authors/maintainers
```

**Validation Criteria:**
- ✅ No version conflicts in dependency tree
- ✅ All transitive dependencies secure
- ✅ No orphaned dependencies

---

### Phase 4: Performance & Optimization

#### 4.1 Build Performance Analysis
Measure and verify build efficiency:

```powershell
# Clean build with timing
$timer = [System.Diagnostics.Stopwatch]::StartNew()
dotnet clean
dotnet build --configuration Release
$timer.Stop()

Write-Host "Build completed in $($timer.ElapsedMilliseconds)ms"
```

**Validation Criteria:**
- ✅ Release build completes in < 90 seconds
- ✅ No unnecessary rebuilds triggered
- ✅ Incremental builds < 10 seconds
- ✅ No build warnings (framework warnings acceptable)

#### 4.2 Test Execution Performance
Verify test efficiency:

```powershell
dotnet test --configuration Release --logger "console" | Measure-Object -Property ElapsedMilliseconds
```

**Validation Criteria:**
- ✅ All 51 tests complete in < 15 seconds
- ✅ No individual test exceeds 500ms
- ✅ No timeout failures
- ✅ Consistent test execution time (variance < 20%)

#### 4.3 Application Runtime Performance
Validate calculator execution speed:

```powershell
# Time basic calculator operations
$operations = @(
    "Add: 1000000 + 2000000",
    "Multiply: 999999 * 999999",
    "Divide: 1000000 / 3",
    "Power: 1000 ^ 100",
    "Modulo: 1000000 % 7"
)

# Verify all operations complete within acceptable time
```

**Validation Criteria:**
- ✅ All operations complete within 100ms
- ✅ No memory leaks (console app cleanup)
- ✅ Large number precision maintained
- ✅ Floating-point calculations accurate to 10 decimal places

---

### Phase 5: Documentation & Compliance

#### 5.1 Code Documentation Validation
Check XML documentation completeness:

```csharp
// ✅ All public classes documented
// ✅ All public methods have <summary>, <param>, <returns>
// ✅ Complex logic has inline comments
// ✅ <exception> documented for thrown exceptions
// ✅ Examples provided where helpful
```

**Validation Criteria:**
- ✅ 100% of public members documented
- ✅ All parameters documented
- ✅ Return values documented
- ✅ Exception types documented
- ✅ No TODO/FIXME comments without context

#### 5.2 README & Guide Validation
Verify documentation is current:

```markdown
✅ README.md includes:
  - Project purpose and overview
  - Quick start instructions
  - Build and test commands
  - Troubleshooting section
  - Links to related documentation

✅ Copilot Instructions:
  - Reflects current .NET framework (net8.0)
  - Package versions current
  - Examples work and are tested
  - Outdated information removed
```

**Validation Criteria:**
- ✅ README is accurate and current
- ✅ All code examples are tested and work
- ✅ Documentation reflects current version
- ✅ No broken links
- ✅ Clear contribution guidelines

#### 5.3 Commit Message Quality
Validate git history:

```powershell
git log --oneline -20 | ForEach-Object {
    # Verify format: topic(subtopic): activity
    # Verify: Present tense, imperative mood
    # Verify: < 72 characters
    # Verify: Related to actual changes
}
```

**Validation Criteria:**
- ✅ All commits follow `topic(subtopic): activity` format
- ✅ Commit messages are descriptive
- ✅ No commits with vague messages ("fix", "update")
- ✅ Each commit is logically cohesive

---

### Phase 6: Project Standards Compliance

#### 6.1 .NET Framework & SDK Compliance
Verify framework requirements:

```powershell
# Check target framework
$projects = @(
    "calculator/calculator.csproj",
    "calculator.tests/calculator.tests.csproj"
)

foreach ($project in $projects) {
    [xml]$content = Get-Content $project
    $targetFramework = $content.Project.PropertyGroup.TargetFramework
    if ($targetFramework -eq "net8.0") {
        Write-Host "✅ $project targets net8.0"
    } else {
        Write-Host "❌ $project targets $targetFramework (expected net8.0)"
    }
}

# Verify SDK version (if global.json exists)
if (Test-Path "global.json") {
    $globalJson = Get-Content global.json | ConvertFrom-Json
    Write-Host "SDK version: $($globalJson.sdk.version)"
}
```

**Validation Criteria:**
- ✅ Both projects target net8.0
- ✅ ImplicitUsings enabled
- ✅ Nullable enabled
- ✅ IsPackable = false for test project
- ✅ SuppressTfmSupportBuildErrors = true (if xUnit conflict exists)

#### 6.2 Project Structure Compliance
Verify directory organization:

```
calculator-xunit-testing/
├── calculator/
│   ├── calculator.csproj ✅
│   ├── Calculator.cs ✅
│   ├── Program.cs ✅
│   └── bin/, obj/ ✅
├── calculator.tests/
│   ├── calculator.tests.csproj ✅
│   ├── CalculatorTest.cs ✅
│   ├── TestData/
│   │   └── CalculatorTestData.csv ✅
│   └── bin/, obj/ ✅
└── calculator-xunit-testing.slnx ✅
```

**Validation Criteria:**
- ✅ Directory structure matches PRD specification
- ✅ File naming follows conventions
- ✅ Test data in dedicated TestData folder
- ✅ No extraneous files in project root

#### 6.3 Project File (.csproj) Validation
Verify MSBuild configuration:

```xml
✅ PropertyGroup:
  - TargetFramework: net8.0
  - OutputType: Exe (calculator), default (calculator.tests)
  - ImplicitUsings: enable
  - Nullable: enable
  - IsPackable: false (test project)
  - SuppressTfmSupportBuildErrors: true (test project)

✅ ItemGroup (PackageReference):
  - xunit Version="2.6.2"
  - xunit.runner.visualstudio Version="2.5.1"
  - Microsoft.NET.Test.Sdk Version="17.5.0"
  - coverlet.collector Version="6.0.0"

✅ ItemGroup (ProjectReference):
  - Test project references app project correctly
```

**Validation Criteria:**
- ✅ All required properties present
- ✅ Package versions exact match PRD
- ✅ Project references valid
- ✅ No duplicate entries
- ✅ Build configuration clean

---

### Phase 7: Integration & End-to-End Testing

#### 7.1 Solution-Wide Build Verification
Test complete solution build:

```powershell
cd calculator-xunit-testing

# Full clean build
dotnet clean
dotnet build --configuration Release --verbosity minimal

# Verify output
if ($LASTEXITCODE -eq 0) {
    Write-Host "✅ Build successful"
    Write-Host "✅ calculator.dll exists: $(Test-Path calculator/bin/Release/net8.0/calculator.dll)"
    Write-Host "✅ calculator.tests.dll exists: $(Test-Path calculator.tests/bin/Release/net8.0/calculator.tests.dll)"
} else {
    Write-Host "❌ Build failed"
}
```

**Validation Criteria:**
- ✅ Clean build succeeds with 0 errors
- ✅ All project outputs created
- ✅ No build warnings (except framework)
- ✅ Incremental build works correctly

#### 7.2 Full Test Suite Execution
Run comprehensive test validation:

```powershell
# Run all tests with reporting
dotnet test --configuration Release `
    --logger "console;verbosity=normal" `
    --collect:"XPlat Code Coverage" `
    --results-directory "./test-results"

# Check results
if ($LASTEXITCODE -eq 0) {
    Write-Host "✅ All tests passed"
} else {
    Write-Host "❌ Some tests failed"
}
```

**Validation Criteria:**
- ✅ 51+ tests pass (0 failures, 0 skips)
- ✅ Code coverage ≥ 80%
- ✅ Test execution time consistent
- ✅ No flaky/intermittent failures

#### 7.3 Calculator Application Manual Verification
Test basic functionality:

```powershell
# Run calculator and verify basic operations
$calculator = & "./calculator/bin/Release/net8.0/calculator.exe"

# Simulate user input:
# - Add: 5 + 3 = 8
# - Subtract: 10 - 4 = 6
# - Multiply: 6 * 7 = 42
# - Divide: 20 / 4 = 5
# - Modulo: 17 % 5 = 2
# - Power: 2 ^ 8 = 256
# - Error: 5 / 0 → exception handled
# - Invalid: 5 & 3 → invalid operator message
```

**Validation Criteria:**
- ✅ All operations calculate correctly
- ✅ Error messages clear and helpful
- ✅ Input validation working
- ✅ Console output formatted properly
- ✅ Program exits gracefully

---

### Phase 8: Quality Gate Summary Report

After completing all phases, generate a comprehensive report:

```markdown
## Quality Assurance Report

### Overall Status: ✅ PASS / ❌ FAIL

| Check | Status | Details |
|:------|:------:|:--------|
| Code Quality | ✅ | 0 violations |
| Test Coverage | ✅ | 85% coverage, 51/51 passing |
| Security | ✅ | 0 CVEs, 0 anti-patterns |
| Performance | ✅ | Build: 60s, Tests: 9s |
| Documentation | ✅ | 100% API documented |
| Compliance | ✅ | Full .NET 8.0 compliance |
| Integration | ✅ | Full solution tested |

### Recommendations:
- If all gates pass: ✅ Ready for merge
- If gates fail: ❌ Address failures before proceeding

### Sign-off:
- Verified by: [Your Name/Role]
- Timestamp: [Date/Time]
- Build ID: [CI/CD Build ID if applicable]
```

**Gate Criteria for Approval:**
- ✅ ALL checks must pass (no exceptions)
- ✅ Coverage ≥ 80%
- ✅ 51+ tests passing
- ✅ Zero security issues
- ✅ Zero build errors
- ✅ Full documentation

---

## Execution Instructions

### Run Quality Checks Manually
```powershell
# Navigate to solution
cd src/workspace/calculator-xunit-testing

# Execute full QA workflow
dotnet clean
dotnet build --configuration Release /p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true
dotnet test --configuration Release --collect:"XPlat Code Coverage"
dotnet format --verify-no-changes
```

### Run Specific Checks
```powershell
# Code quality only
dotnet build /p:EnableNETAnalyzers=true

# Tests only
dotnet test --configuration Release

# Coverage report only
dotnet test --collect:"XPlat Code Coverage"
```

---

## Success Criteria Summary

✅ **Quality gate passes when ALL criteria met:**

| Category | Metric | Target |
|:---------|:------:|:------:|
| **Code Quality** | Code style violations | 0 |
| **Code Quality** | Cyclomatic complexity | < 10 |
| **Tests** | Pass rate | 100% |
| **Tests** | Code coverage | ≥ 80% |
| **Tests** | Test count | 51+ |
| **Security** | CVEs found | 0 |
| **Security** | Anti-patterns | 0 |
| **Performance** | Build time | < 90s |
| **Performance** | Test time | < 15s |
| **Documentation** | API docs | 100% |
| **Compliance** | Framework | net8.0 |
| **Compliance** | Standards | ✅ Compliant |

---

## SW Tester/QA Engineer Persona Capabilities

This prompt is specifically designed to enable SW Tester/QA Engineer personas with the following GitHub Copilot-enhanced capabilities:

### 🧪 Core QA Capabilities

| Capability | Implementation | Benefit |
|:-----------|:---------------|:--------|
| **Automated Test Case Generation** | Phase 2.5.1 | Rapidly generate comprehensive test cases for all code paths |
| **Edge Case Suggestions** | Phase 2.5.2 | Identify untested scenarios and boundary conditions automatically |
| **Test Data Generation** | Phase 2.5.3 | Create realistic, comprehensive test datasets programmatically |
| **Bug Reproduction Assistance** | Phase 2.5.4 | Generate reproducible test cases for known issues |
| **Mock Data & Stubbing** | Phase 2.5.5 | Create reusable test fixtures and mock objects |
| **Regression Test Writing** | Phase 2.5.6 | Document and test all previously fixed issues |
| **Code Coverage Improvement** | Phase 2.5.7 | Identify gaps and generate tests for uncovered code |
| **Automation Scripting** | Phase 2.5.8 | Build test orchestration and reporting frameworks |

### 📊 QA Engineer Workflow

```
1. Automated Test Generation (Phase 2.5.1)
   ↓
2. Edge Case Analysis (Phase 2.5.2)
   ↓
3. Test Data Expansion (Phase 2.5.3)
   ↓
4. Bug Reproduction Testing (Phase 2.5.4)
   ↓
5. Mock Data Setup (Phase 2.5.5)
   ↓
6. Regression Test Creation (Phase 2.5.6)
   ↓
7. Coverage Gap Elimination (Phase 2.5.7)
   ↓
8. Test Automation Framework (Phase 2.5.8)
   ↓
9. Quality Gate Enforcement (Phase 8)
```

### 🎯 QA-Specific Success Metrics

| Metric | Target | Phase |
|:-------|:------:|:-----:|
| Test Case Coverage | All code paths | 2.5.1 |
| Edge Cases Identified | 100% | 2.5.2 |
| Test Data Variety | ≥ 50 scenarios | 2.5.3 |
| Bug Reproducibility | 100% | 2.5.4 |
| Mock Fixture Coverage | All dependencies | 2.5.5 |
| Regression Tests | All fixed issues | 2.5.6 |
| Code Coverage Gaps | 0% uncovered paths | 2.5.7 |
| Test Automation | Full CI/CD integration | 2.5.8 |

### 💡 QA Efficiency Gains

- **50% faster test case creation** – Automated generation vs. manual writing
- **80% better edge case coverage** – AI-suggested boundary values and scenarios
- **Reduced false positives** – Precise, data-driven test fixtures
- **Faster bug reproduction** – Automated test case generation from bug reports
- **100% regression prevention** – Systematic regression test coverage
- **Improved maintainability** – Well-organized, reusable test assets

---

## References

- [Microsoft C# Coding Conventions](https://docs.microsoft.com/en-us/dotnet/csharp/fundamentals/coding-style/coding-conventions)
- [xUnit Testing Best Practices](https://xunit.net/)
- [Code Coverage Analysis](https://github.com/coverlet-coverage/coverlet)
- [OWASP Security Testing Guide](https://owasp.org/www-project-web-security-testing-guide/)

