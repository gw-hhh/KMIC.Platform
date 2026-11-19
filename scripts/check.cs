#:property RestorePackagesWithLockFile=false

// 本地和 CI 共用的质量检查：dotnet run scripts/check.cs
// 按下方顺序执行，任一步失败即停止，并返回该步骤的退出码。
// 数据库测试和密钥扫描需要 Docker。
// 本地可加 -- --fix 移除 BOM，并将 CRLF 换行改为 LF；CI 只检查，不修复。
// 依赖漏洞扫描在本地依赖锁文件未变时 24 小时内只扫一次，CI 每次都扫。
using System.Diagnostics;
using System.Globalization;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;

var root = FindRepositoryRoot();
var fix = args.Contains("--fix", StringComparer.Ordinal);
var steps = new (string Name, Func<int> Run)[]
{
    ("编码与换行（UTF-8 无 BOM、LF）", () => CheckEncoding(root, fix)),
    ("临时引用标记", () => CheckTemporaryReferences(root)),
    // 代码风格与分析器规则由编译检查（EnforceCodeStyleInBuild、警告即错误），这里只查编译不管的空白、文件末尾换行与 using 顺序
    ("格式（空白与 using 顺序）", () => RunWhenProjectsExist(root, "dotnet", "format Kmic.slnx --verify-no-changes --diagnostics IMPORTS")),
    ("编译（警告即错误）", () => RunWhenProjectsExist(root, "dotnet", "build Kmic.slnx --nologo")),
    ("测试", () => RunWhenProjectsExist(root, "dotnet", "test --solution Kmic.slnx --no-build")),
    ("依赖漏洞（含传递依赖）", () => WhenProjectsExist(root, () => CheckVulnerabilities(root))),
    // pnpm 版本与 web/package.json 保持一致。
    // 接口类型比对依赖后端测试导出的 OpenAPI 文档，须在后端测试后执行。
    ("前端依赖（pnpm install --frozen-lockfile）", () => RunWeb(root, "install --frozen-lockfile")),
    ("前端接口类型比对", () => RunWeb(root, "run generate:api -- --check")),
    ("前端类型检查", () => RunWeb(root, "run typecheck")),
    ("前端代码及依赖边界检查", () => RunWeb(root, "run lint")),
    ("前端格式（prettier --check）", () => RunWeb(root, "run format:check")),
    ("前端测试", () => RunWeb(root, "run test")),
    ("前端构建", () => RunWeb(root, "run build")),
    // 密钥扫描包含提交历史，CI 必须检出完整历史。
    ("密钥扫描（Gitleaks）", () => Run(root, "docker", $"run --rm -v \"{root}:/repo\" ghcr.io/gitleaks/gitleaks:v8.30.1 git /repo --config /repo/.gitleaks.toml --redact --no-banner")),
    ("拼写（cspell）", () => Run(root, "npx", "--yes cspell@10.4.0 lint --no-progress --no-must-find-files --config cspell.json")),
};

var total = Stopwatch.StartNew();
foreach (var (name, run) in steps)
{
    Console.WriteLine($"== {name}");
    var step = Stopwatch.StartNew();
    var exitCode = run();
    Console.WriteLine($"   用时 {Seconds(step.Elapsed)} 秒");
    if (exitCode != 0)
    {
        Console.WriteLine($"== 失败：{name}（退出码 {exitCode}）");
        return exitCode;
    }
}

Console.WriteLine($"== 全部通过（共 {Seconds(total.Elapsed)} 秒）");
return 0;

static int Seconds(TimeSpan elapsed) => (int)Math.Round(elapsed.TotalSeconds);

static string FindRepositoryRoot()
{
    var directory = new DirectoryInfo(Environment.CurrentDirectory);
    while (directory is not null && !File.Exists(Path.Combine(directory.FullName, "Kmic.slnx")))
    {
        directory = directory.Parent;
    }

    return directory?.FullName ?? throw new InvalidOperationException("找不到 Kmic.slnx，请在仓库目录里运行。");
}

static int RunWhenProjectsExist(string root, string fileName, string arguments) =>
    WhenProjectsExist(root, () => Run(root, fileName, arguments));

static int WhenProjectsExist(string root, Func<int> run)
{
    var solution = File.ReadAllText(Path.Combine(root, "Kmic.slnx"));
    if (!solution.Contains("<Project ", StringComparison.Ordinal))
    {
        Console.WriteLine("   解决方案中没有项目，跳过 .NET 检查");
        return 0;
    }

    return run();
}

// dotnet list package --vulnerable 发现漏洞时退出码仍是 0，所以读它的 JSON 输出判断；扫描本身报告问题时同样判失败，避免没扫成也算通过。
// 扫描要联网，约一分半：本地在依赖锁文件没变、上次扫描通过且不满 24 小时时跳过，CI 每次都扫
static int CheckVulnerabilities(string root)
{
    var ci = string.Equals(Environment.GetEnvironmentVariable("CI"), "true", StringComparison.OrdinalIgnoreCase);
    var record = Path.Combine(root, "artifacts", "check", "vulnerability-scan.txt");
    var fingerprint = LockFilesFingerprint(root);
    if (!ci && LastPassedScan(record, fingerprint) is { } scannedAt && TimeProvider.System.GetUtcNow() - scannedAt < TimeSpan.FromHours(24))
    {
        Console.WriteLine($"   依赖锁文件自上次扫描（{scannedAt.ToLocalTime():yyyy-MM-dd HH:mm}）后没有变化，24 小时内不重复扫描");
        return 0;
    }

    var (exitCode, output) = Capture(root, "dotnet", "list Kmic.slnx package --vulnerable --include-transitive --format json");
    if (exitCode != 0)
    {
        Console.WriteLine(output);
        return exitCode;
    }

    using var report = JsonDocument.Parse(output);
    var problems = 0;
    foreach (var problem in Items(report.RootElement, "problems"))
    {
        Console.WriteLine($"   扫描出错：{problem}");
        problems++;
    }

    foreach (var project in Items(report.RootElement, "projects"))
    {
        var name = Path.GetFileNameWithoutExtension(project.GetProperty("path").GetString());
        foreach (var problem in Items(project, "problems"))
        {
            Console.WriteLine($"   {name} 扫描出错：{problem}");
            problems++;
        }

        var packages = Items(project, "frameworks").SelectMany(f => Items(f, "topLevelPackages").Concat(Items(f, "transitivePackages")));
        foreach (var package in packages)
        {
            foreach (var vulnerability in Items(package, "vulnerabilities"))
            {
                Console.WriteLine($"   {name}：{package.GetProperty("id").GetString()} {package.GetProperty("resolvedVersion").GetString()}，{vulnerability.GetProperty("severity").GetString()}，{vulnerability.GetProperty("advisoryurl").GetString()}");
                problems++;
            }
        }
    }

    if (problems > 0)
    {
        Console.WriteLine($"   发现 {problems} 处漏洞或扫描问题");
        return 1;
    }

    Directory.CreateDirectory(Path.GetDirectoryName(record)!);
    File.WriteAllLines(record, [fingerprint, TimeProvider.System.GetUtcNow().ToString("O", CultureInfo.InvariantCulture)]);
    Console.WriteLine("   没有已知漏洞");
    return 0;
}

// 上次扫描通过的时间；锁文件变了或没有记录时为空
static DateTimeOffset? LastPassedScan(string record, string fingerprint)
{
    if (!File.Exists(record))
    {
        return null;
    }

    var lines = File.ReadAllLines(record);
    return lines.Length == 2
        && string.Equals(lines[0], fingerprint, StringComparison.Ordinal)
        && DateTimeOffset.TryParse(lines[1], CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind, out var scannedAt)
            ? scannedAt
            : null;
}

// 全部 packages.lock.json 的路径与内容的摘要，任何依赖的版本变化都会改变它
static string LockFilesFingerprint(string root)
{
    using var hash = IncrementalHash.CreateHash(HashAlgorithmName.SHA256);
    var lockFiles = ListFiles(root)
        .Where(f => string.Equals(Path.GetFileName(f), "packages.lock.json", StringComparison.Ordinal) && File.Exists(Path.Combine(root, f)))
        .Order(StringComparer.Ordinal);
    foreach (var file in lockFiles)
    {
        hash.AppendData(Encoding.UTF8.GetBytes(file + "\n"));
        hash.AppendData(File.ReadAllBytes(Path.Combine(root, file)));
    }

    return Convert.ToHexString(hash.GetHashAndReset());
}

static IEnumerable<JsonElement> Items(JsonElement element, string name) =>
    element.TryGetProperty(name, out var items) && items.ValueKind == JsonValueKind.Array ? items.EnumerateArray() : [];

static (int ExitCode, string Output) Capture(string root, string fileName, string arguments)
{
    var startInfo = new ProcessStartInfo(fileName, arguments)
    {
        WorkingDirectory = root,
        UseShellExecute = false,
        RedirectStandardOutput = true,
        StandardOutputEncoding = Encoding.UTF8,
    };
    using var process = Process.Start(startInfo) ?? throw new InvalidOperationException($"无法启动 {fileName}。");
    var output = process.StandardOutput.ReadToEnd();
    process.WaitForExit();
    return (process.ExitCode, output);
}

static int RunWeb(string root, string pnpmArguments)
{
    if (!Directory.Exists(Path.Combine(root, "web")))
    {
        Console.WriteLine("   未找到 web 目录，跳过前端检查");
        return 0;
    }

    return Run(root, "npx", $"--yes pnpm@11.28.9 --dir web {pnpmArguments}");
}

static int Run(string root, string fileName, string arguments)
{
    // Windows 上的 npx 是批处理命令，需要通过 cmd.exe 执行。
    var startInfo = OperatingSystem.IsWindows() && string.Equals(fileName, "npx", StringComparison.Ordinal)
        ? new ProcessStartInfo("cmd.exe", $"/c npx {arguments}")
        : new ProcessStartInfo(fileName, arguments);
    startInfo.WorkingDirectory = root;
    startInfo.UseShellExecute = false;
    using var process = Process.Start(startInfo) ?? throw new InvalidOperationException($"无法启动 {fileName}。");
    process.WaitForExit();
    return process.ExitCode;
}

static int CheckEncoding(string root, bool fix)
{
    var files = ListFiles(root);

    string[] binaryExtensions = [".png", ".jpg", ".jpeg", ".gif", ".webp", ".ico", ".pdf", ".xlsx", ".docx", ".zip", ".woff", ".woff2"];
    var problems = 0;
    var fixedFiles = 0;
    foreach (var file in files)
    {
        if (binaryExtensions.Contains(Path.GetExtension(file), StringComparer.OrdinalIgnoreCase))
        {
            continue;
        }

        // Windows 上的 NuGet 锁文件使用 CRLF，由 Git 入库时转为 LF。
        if (string.Equals(Path.GetFileName(file), "packages.lock.json", StringComparison.OrdinalIgnoreCase))
        {
            continue;
        }

        var path = Path.Combine(root, file);
        if (!File.Exists(path))
        {
            continue;
        }

        var bytes = File.ReadAllBytes(path);
        var hasBom = bytes.AsSpan().StartsWith("﻿"u8);
        var hasCrlf = bytes.AsSpan().IndexOf("\r\n"u8) >= 0;
        if (!hasBom && !hasCrlf)
        {
            continue;
        }

        if (fix)
        {
            var text = Encoding.UTF8.GetString(hasBom ? bytes.AsSpan(3) : bytes);
            File.WriteAllText(path, text.Replace("\r\n", "\n", StringComparison.Ordinal), new UTF8Encoding(encoderShouldEmitUTF8Identifier: false));
            Console.WriteLine($"   已修复：{file}");
            fixedFiles++;
            continue;
        }

        if (hasBom)
        {
            Console.WriteLine($"   含 UTF-8 BOM：{file}");
            problems++;
        }

        if (hasCrlf)
        {
            Console.WriteLine($"   换行应为 LF：{file}");
            problems++;
        }
    }

    Console.WriteLine(fix
        ? $"   已修复 {fixedFiles} 个文件"
        : $"   发现 {problems} 处编码或换行问题");
    return problems == 0 ? 0 : 1;
}

// 列出已跟踪文件和未被忽略的新文件；返回以 / 分隔的仓库相对路径。
static string[] ListFiles(string root)
{
    var startInfo = new ProcessStartInfo("git", "-c core.quotepath=off ls-files -z --cached --others --exclude-standard")
    {
        WorkingDirectory = root,
        UseShellExecute = false,
        RedirectStandardOutput = true,
        StandardOutputEncoding = Encoding.UTF8,
    };
    using var git = Process.Start(startInfo) ?? throw new InvalidOperationException("无法启动 git。");
    var files = git.StandardOutput.ReadToEnd().Split('\0', StringSplitOptions.RemoveEmptyEntries);
    git.WaitForExit();
    return files;
}

static int CheckTemporaryReferences(string root)
{
    string[] extensions = [".cs", ".ts", ".vue"];
    var problems = 0;
    foreach (var file in ListFiles(root))
    {
        // 脚本中包含旧标记的示例，跳过自身。
        if (!extensions.Contains(Path.GetExtension(file), StringComparer.OrdinalIgnoreCase)
            || string.Equals(file, "scripts/check.cs", StringComparison.Ordinal))
        {
            continue;
        }

        var path = Path.Combine(root, file);
        if (!File.Exists(path))
        {
            continue;
        }

        var lines = File.ReadAllLines(path);
        for (var i = 0; i < lines.Length; i++)
        {
            if (Kmic.Scripts.Patterns.TemporaryReference().IsMatch(lines[i]))
            {
                Console.WriteLine($"   {file}:{i + 1} 请移除旧引用标记，直接写明用途或原因");
                problems++;
            }
        }
    }

    Console.WriteLine($"   发现 {problems} 处旧引用标记");
    return problems == 0 ? 0 : 1;
}

namespace Kmic.Scripts
{
    internal static partial class Patterns
    {
        // 只匹配“【引用：”或“【引用:”，允许冒号前有空白。
        [GeneratedRegex(@"【引用\s*[:：]", RegexOptions.CultureInvariant, matchTimeoutMilliseconds: 1000)]
        public static partial Regex TemporaryReference();
    }
}
