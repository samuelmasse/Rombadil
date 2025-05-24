Once("ReportGenerator", () =>
{
    Run("dotnet tool install --global dotnet-reportgenerator-globaltool",
        "Ensuring ReportGenerator is installed",
        "Failed to install ReportGenerator");
});

Dir("out", "TestResults", out var testResultsDir);

Section(() =>
{
    Info($"Cleaning previous test results in {testResultsDir}");
    Delete(testResultsDir);
});

Section(() =>
{
    Run($"dotnet test {(args.Length >= 1 ? args[0] : ".")} --results-directory {testResultsDir} --collect:\"XPlat Code Coverage\"",
        "Running tests with coverage collection",
        "Failed to run tests");
});

Section(() =>
{
    Dir("out", "CoverageReport", out var coverageReportDir);
    Dir("out", "CoverageHistory", out var coverageHistoryDir);
    Dir(testResultsDir, "**", "coverage.cobertura.xml", out var coverageFile);

    Run($"reportgenerator -reports:{coverageFile} -targetdir:{coverageReportDir} -historydir:{coverageHistoryDir}",
        "Generating coverage report with history tracking",
        "Failed to generate coverage report");

    Dir(coverageReportDir, "index.html", out var coverageReport);
    Success($"Coverage report generated successfully : {Link(coverageReport)}");
});
