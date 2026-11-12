#:property RestorePackagesWithLockFile=false

// 新建业务模块：dotnet run scripts/new-module.cs -- <模块名> <编号>
// 先在 docs/modules/README.md 的登记表里登记模块名与编号，再运行本脚本。依次执行：
// 对照登记表校验 → dotnet new kmic-module → 加入 Kmic.slnx → 宿主与架构测试引用新模块并登记 →
// 编译 → 生成初始迁移（补 InitializeModuleSchema）→ 规整 dotnet 工具写出的换行与 BOM。
// 之后运行 dotnet run scripts/check.cs -- --fix 做完整检查
using System.Diagnostics;
using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;

if (args.Length != 2 || !int.TryParse(args[1], CultureInfo.InvariantCulture, out var number))
{
    Console.WriteLine("用法：dotnet run scripts/new-module.cs -- <模块名> <编号>");
    return 2;
}

var module = args[0];
var root = FindRepositoryRoot();
var problems = Validate(root, module, number);
if (problems.Count > 0)
{
    problems.ForEach(p => Console.WriteLine($"   {p}"));
    return 1;
}

// 显式指定解决方案：dotnet sln 默认向上查找，在旧仓库里会改到别的解决方案
var steps = new (string Name, Func<int> Run)[]
{
    ("安装并运行模板", () => Template(root, module)),
    ("加入解决方案", () => AddToSolution(root, module)),
    ("宿主与架构测试引用新模块并登记", () => Register(root, module)),
    ("编译", () => Run(root, "dotnet", "build Kmic.slnx --nologo")),
    ("生成初始迁移", () => Migration(root, module)),
    ("规整换行与 BOM", () => Normalize(root, module)),
};

foreach (var (name, run) in steps)
{
    Console.WriteLine($"== {name}");
    var exitCode = run();
    if (exitCode != 0)
    {
        Console.WriteLine($"== 失败：{name}（退出码 {exitCode}）");
        return exitCode;
    }
}

Console.WriteLine($"== 模块 {module} 已建好。下一步：写设计文档、按用例逐个实现，并运行 dotnet run scripts/check.cs -- --fix");
return 0;

static string FindRepositoryRoot()
{
    var directory = new DirectoryInfo(Environment.CurrentDirectory);
    while (directory is not null && !File.Exists(Path.Combine(directory.FullName, "Kmic.slnx")))
    {
        directory = directory.Parent;
    }

    return directory?.FullName ?? throw new InvalidOperationException("找不到 Kmic.slnx，请在仓库目录里运行。");
}

// 名称与编号的规则，以及它们在登记表里已经登记、没有被占用
static List<string> Validate(string root, string module, int number)
{
    var problems = new List<string>();
    if (!Regex.IsMatch(module, "^[A-Z][A-Za-z0-9]*$", RegexOptions.CultureInvariant, TimeSpan.FromSeconds(1)))
    {
        problems.Add("模块名要写成大驼峰的单个英文名词，如 Sales。");
    }

    if (string.Equals(module, "BuildingBlocks", StringComparison.Ordinal) || string.Equals(module, "Customer", StringComparison.Ordinal))
    {
        problems.Add($"{module} 是保留名。");
    }

    if (number is < 10 or > 99 or (> 89 and < 99))
    {
        problems.Add("编号：10～19 平台模块，20～89 业务模块，99 参考模块。");
    }

    var registry = File.ReadAllLines(Path.Combine(root, "docs", "modules", "README.md"));
    var row = registry.FirstOrDefault(l => l.StartsWith($"| {module} |", StringComparison.Ordinal));
    if (row is null)
    {
        problems.Add($"先在 docs/modules/README.md 的登记表里登记 {module}。");
    }
    else if (!row.Contains($"| {number} |", StringComparison.Ordinal))
    {
        problems.Add($"登记表里 {module} 的编号不是 {number}。");
    }

    var others = registry.Where(l => l.Contains($"| {number} |", StringComparison.Ordinal) && !l.StartsWith($"| {module} |", StringComparison.Ordinal));
    if (others.Any())
    {
        problems.Add($"编号 {number} 已被其他模块占用。");
    }

    if (Directory.Exists(Path.Combine(root, "src", "Modules", module)))
    {
        problems.Add($"src/Modules/{module} 已经存在。");
    }

    return problems;
}

static int Template(string root, string module)
{
    var install = Run(root, "dotnet", "new install templates/kmic-module --force");
    return install != 0 ? install : Run(root, "dotnet", $"new kmic-module -n {module} -o . --force");
}

static string[] ProjectsOf(string module) =>
[
    $"src/Modules/{module}/Kmic.{module}.Contracts/Kmic.{module}.Contracts.csproj",
    $"src/Modules/{module}/Kmic.{module}.Domain/Kmic.{module}.Domain.csproj",
    $"src/Modules/{module}/Kmic.{module}.Application/Kmic.{module}.Application.csproj",
    $"src/Modules/{module}/Kmic.{module}.Infrastructure/Kmic.{module}.Infrastructure.csproj",
    $"src/Modules/{module}/Kmic.{module}.Presentation/Kmic.{module}.Presentation.csproj",
    $"tests/Modules/{module}/Kmic.{module}.Domain.Tests/Kmic.{module}.Domain.Tests.csproj",
];

static int AddToSolution(string root, string module)
{
    var projects = string.Join(' ', ProjectsOf(module));
    return Run(root, "dotnet", $"sln Kmic.slnx add {projects}");
}

static int Register(string root, string module)
{
    // 宿主引用服务端与接口层两个项目，并在 Program.cs 里登记
    var api = Path.Combine(root, "src", "Hosts", "Kmic.Api", "Kmic.Api.csproj");
    InsertAfterLast(api, "<ProjectReference ", $"""
        <ProjectReference Include="../../Modules/{module}/Kmic.{module}.Infrastructure/Kmic.{module}.Infrastructure.csproj" />
        <ProjectReference Include="../../Modules/{module}/Kmic.{module}.Presentation/Kmic.{module}.Presentation.csproj" />
        """);

    // 架构测试按项目列出引用，新模块的五个层都要加
    var architecture = Path.Combine(root, "tests", "Kmic.ArchitectureTests", "Kmic.ArchitectureTests.csproj");
    var layers = new[] { "Domain", "Contracts", "Application", "Infrastructure", "Presentation" };
    InsertAfterLast(architecture, "<ProjectReference ", string.Join(Environment.NewLine, layers.Select(l =>
        $"""    <ProjectReference Include="../../src/Modules/{module}/Kmic.{module}.{l}/Kmic.{module}.{l}.csproj" />""")));

    var program = Path.Combine(root, "src", "Hosts", "Kmic.Api", "Program.cs");
    var text = File.ReadAllText(program);
    text = text.Replace("using Kmic.BuildingBlocks.Presentation.OpenApi;", $"using Kmic.BuildingBlocks.Presentation.OpenApi;{Environment.NewLine}using Kmic.{module}.Infrastructure;{Environment.NewLine}using Kmic.{module}.Presentation;", StringComparison.Ordinal);
    text = text.Replace("builder.AddKmicHost(host => host", $"builder.AddKmicHost(host => host{Environment.NewLine}    .AddModule<{module}Module, {module}Endpoints>()", StringComparison.Ordinal);
    File.WriteAllText(program, text);
    return Run(root, "dotnet", "restore Kmic.slnx --force-evaluate");
}

// 在文件里最后一个包含 marker 的行后面插入新行，缩进与那一行相同
static void InsertAfterLast(string file, string marker, string newLines)
{
    var lines = File.ReadAllLines(file).ToList();
    var index = lines.FindLastIndex(l => l.Contains(marker, StringComparison.Ordinal));
    var indent = new string(' ', lines[index].Length - lines[index].TrimStart().Length);
    lines.InsertRange(index + 1, newLines.Split('\n', StringSplitOptions.TrimEntries).Select(l => indent + l.Trim()));
    File.WriteAllLines(file, lines);
}

// dotnet ef 要可执行的启动项目：用宿主。生成后在 Up 开头补模块 schema 的初始化与授权
static int Migration(string root, string module)
{
    var arguments = $"ef migrations add InitializeModuleSchema --project src/Modules/{module}/Kmic.{module}.Infrastructure --startup-project src/Hosts/Kmic.Api --context {module}DbContext --output-dir Persistence/Migrations";
    var exitCode = Run(root, "dotnet", arguments);
    if (exitCode != 0)
    {
        return exitCode;
    }

    var directory = Path.Combine(root, "src", "Modules", module, $"Kmic.{module}.Infrastructure", "Persistence", "Migrations");
    var migration = Directory.GetFiles(directory, "*_InitializeModuleSchema.cs").Single(f => !f.EndsWith(".Designer.cs", StringComparison.Ordinal));
    var text = File.ReadAllText(migration);
    text = text.Replace("using System;", $"using System;{Environment.NewLine}using Kmic.BuildingBlocks.Infrastructure.Persistence;", StringComparison.Ordinal);
    text = Regex.Replace(text, @"(?<head>protected override void Up\(MigrationBuilder migrationBuilder\)\s*\{)", $"${{head}}{Environment.NewLine}            migrationBuilder.InitializeModuleSchema(\"{module}\");{Environment.NewLine}", RegexOptions.CultureInvariant, TimeSpan.FromSeconds(1));
    File.WriteAllText(migration, text);
    return Run(root, "dotnet", "build Kmic.slnx --nologo");
}

// dotnet 工具按 CRLF 写文件（迁移还带 BOM）：新建的模块、改过的文件统一成 UTF-8 无 BOM、LF
static int Normalize(string root, string module)
{
    var files = Directory.GetFiles(Path.Combine(root, "src", "Modules", module), "*", SearchOption.AllDirectories)
        .Concat(Directory.GetFiles(Path.Combine(root, "tests", "Modules", module), "*", SearchOption.AllDirectories))
        .Concat([
            Path.Combine(root, "Kmic.slnx"),
            Path.Combine(root, $"{module}.slnf"),
            Path.Combine(root, "src", "Hosts", "Kmic.Api", "Kmic.Api.csproj"),
            Path.Combine(root, "src", "Hosts", "Kmic.Api", "Program.cs"),
            Path.Combine(root, "tests", "Kmic.ArchitectureTests", "Kmic.ArchitectureTests.csproj"),
        ])
        .Where(f => !f.Contains($"{Path.DirectorySeparatorChar}obj{Path.DirectorySeparatorChar}", StringComparison.Ordinal)
            && !f.Contains($"{Path.DirectorySeparatorChar}bin{Path.DirectorySeparatorChar}", StringComparison.Ordinal)
            && File.Exists(f));
    foreach (var file in files)
    {
        var text = File.ReadAllText(file).Replace("\r\n", "\n", StringComparison.Ordinal);
        File.WriteAllText(file, text, new UTF8Encoding(encoderShouldEmitUTF8Identifier: false));
    }

    return Run(root, "dotnet", "format Kmic.slnx");
}

static int Run(string root, string fileName, string arguments)
{
    var startInfo = new ProcessStartInfo(fileName, arguments) { WorkingDirectory = root, UseShellExecute = false };
    using var process = Process.Start(startInfo) ?? throw new InvalidOperationException($"无法启动 {fileName}。");
    process.WaitForExit();
    return process.ExitCode;
}
