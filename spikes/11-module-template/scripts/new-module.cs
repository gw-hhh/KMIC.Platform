#:property RestorePackagesWithLockFile=false

// 新建模块：dotnet run new-module.cs -- <模块名> <编号> <仓库根目录>
// 依次执行：校验 → 模板生成 → 加入 Kmic.slnx → 用 .slnf 构建 → 生成初始迁移 → 格式检查 → 运行模块测试；任一步失败即停止
// 原型只校验名称与编号的格式，正式脚本还要对照模块登记表检查名称与编号未被占用
using System.Diagnostics;
using System.Globalization;

if (args is not [var name, var numberText, var root])
{
    Console.Error.WriteLine("用法：dotnet run new-module.cs -- <模块名> <编号> <仓库根目录>");
    return 2;
}

if (name is not [var first, .. var rest] || !char.IsAsciiLetterUpper(first) || rest.Length == 0 || !rest.All(char.IsAsciiLetterLower))
{
    Console.Error.WriteLine($"模块名必须是单个英文名词、大驼峰：{name}");
    return 2;
}

if (!int.TryParse(numberText, CultureInfo.InvariantCulture, out var number) || number is < 10 or > 99)
{
    Console.Error.WriteLine($"模块编号必须是 10～99 的两位数字：{numberText}");
    return 2;
}

var moduleDirectory = Path.Combine(root, "src", "Modules", name);
if (Directory.Exists(moduleDirectory))
{
    Console.Error.WriteLine($"模块目录已存在：{moduleDirectory}");
    return 2;
}

var template = Path.GetFullPath(Path.Combine(AppContext.GetData("EntryPointFileDirectoryPath") as string ?? ".", "..", "templates", "kmic-module"));
var infrastructure = $"src/Modules/{name}/Kmic.{name}.Infrastructure";
string[] projects =
[
    $"src/Modules/{name}/Kmic.{name}.Domain/Kmic.{name}.Domain.csproj",
    $"src/Modules/{name}/Kmic.{name}.Contracts/Kmic.{name}.Contracts.csproj",
    $"src/Modules/{name}/Kmic.{name}.Application/Kmic.{name}.Application.csproj",
    $"{infrastructure}/Kmic.{name}.Infrastructure.csproj",
    $"src/Modules/{name}/Kmic.{name}.Presentation/Kmic.{name}.Presentation.csproj",
    $"tests/Modules/{name}/Kmic.{name}.IntegrationTests/Kmic.{name}.IntegrationTests.csproj",
];

// 模板自带的“加入解决方案”只认 .sln 且会向上层目录查找，所以显式指定 Kmic.slnx
(string Step, string Arguments)[] steps =
[
    ("安装模板", $"new install \"{template}\" --force"),
    ("生成骨架", $"new kmic-module -n {name} --number {number}"),
    ("加入解决方案", $"sln Kmic.slnx add {string.Join(' ', projects)}"),
    ("构建", $"build src/Modules/{name}/{name}.slnf"),

    // dotnet ef 不会先还原依赖包，所以放在构建之后并跳过它自己的构建；测试时会连同迁移再构建一次
    ("生成初始迁移", $"ef migrations add InitializeModuleSchema -p {infrastructure} -c {name}DbContext -o Persistence/Migrations --no-build"),
    ("格式检查", $"format src/Modules/{name}/{name}.slnf --verify-no-changes"),
    ("测试", $"test --project tests/Modules/{name}/Kmic.{name}.IntegrationTests"),
];

foreach (var (step, arguments) in steps)
{
    Console.WriteLine($"== {step}");
    var exitCode = Run(root, arguments);
    if (exitCode != 0)
    {
        Console.WriteLine($"== 失败：{step}（退出码 {exitCode}）");
        return exitCode;
    }
}

Console.WriteLine($"== 模块 {name}（编号 {number}）已生成");
return 0;

static int Run(string workingDirectory, string arguments)
{
    var startInfo = new ProcessStartInfo("dotnet", arguments) { WorkingDirectory = workingDirectory, UseShellExecute = false };
    using var process = Process.Start(startInfo) ?? throw new InvalidOperationException("Cannot start dotnet.");
    process.WaitForExit();
    return process.ExitCode;
}
