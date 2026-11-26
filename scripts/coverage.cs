#:property RestorePackagesWithLockFile=false

// 测试覆盖率报告：dotnet run scripts/coverage.cs
// 跑一遍全部测试并收集覆盖率，只出报告，不设门槛；需要 Docker
// 报告在 artifacts/coverage/report（Summary.txt 与 index.html），生成代码（实体注释、日志方法、迁移）和测试项目不计入
using System.Diagnostics;

var root = FindRepositoryRoot();
var output = Path.Combine(root, "artifacts", "coverage");
Directory.CreateDirectory(output);
var data = Path.Combine(output, "coverage.cobertura.xml");

Run(root, "dotnet", "tool restore");
Run(root, "dotnet", "build Kmic.slnx --nologo");
Run(root, "dotnet", $"dotnet-coverage collect -f cobertura -o \"{data}\" \"dotnet test --solution Kmic.slnx --no-build\"");
Run(
    root,
    "dotnet",
    $"reportgenerator -reports:\"{data}\" -targetdir:\"{Path.Combine(output, "report")}\" -reporttypes:TextSummary;Html "
    + "\"-assemblyfilters:+Kmic.*;-*Tests*;-Kmic.Echo.Probe\" "
    + "\"-classfilters:-Kmic.Generated.*;-*.Migrations.*;-*Log;-*Log.*\"");
Console.WriteLine(File.ReadAllText(Path.Combine(output, "report", "Summary.txt")));
return 0;

static void Run(string root, string fileName, string arguments)
{
    var startInfo = new ProcessStartInfo(fileName, arguments) { WorkingDirectory = root, UseShellExecute = false };
    using var process = Process.Start(startInfo) ?? throw new InvalidOperationException($"无法启动 {fileName}。");
    process.WaitForExit();
    if (process.ExitCode != 0)
    {
        Environment.Exit(process.ExitCode);
    }
}

static string FindRepositoryRoot()
{
    var directory = new DirectoryInfo(Environment.CurrentDirectory);
    while (directory is not null && !File.Exists(Path.Combine(directory.FullName, "Kmic.slnx")))
    {
        directory = directory.Parent;
    }

    return directory?.FullName ?? throw new InvalidOperationException("找不到 Kmic.slnx，请在仓库内运行。");
}
