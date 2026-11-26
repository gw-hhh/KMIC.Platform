#:property RestorePackagesWithLockFile=false

// 本地和 CI 共用的质量检查：dotnet run scripts/check.cs
// 按下方顺序执行，任一步失败即停止，并返回该步骤的退出码
// 数据库测试和密钥扫描需要 Docker
// 参数（本地用，CI 不加）：
//   --fix-only  只修能自动修的（BOM、CRLF、summary 标签各占一行）然后退出，几秒钟
//   --fix       先修，再跑全部检查
//   --fast      只跑静态检查和单元测试（约一两分钟），跳过 .NET 测试、依赖漏洞、前端构建与端到端、密钥扫描、拼写
// 依赖漏洞扫描在本地依赖锁文件未变时 24 小时内只扫一次，CI 每次都扫
using System.Diagnostics;
using System.Globalization;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;

var root = FindRepositoryRoot();
var fixOnly = args.Contains("--fix-only", StringComparer.Ordinal);
var fix = fixOnly || args.Contains("--fix", StringComparer.Ordinal);
var fast = args.Contains("--fast", StringComparer.Ordinal);

if (fixOnly)
{
    var encoding = CheckEncoding(root, fix: true);
    var comments = CheckCommentStyle(root, fix: true);
    return encoding == 0 && comments == 0 ? 0 : 1;
}

// Fast：--fast 时也跑
var steps = new (string Name, bool Fast, Func<int> Run)[]
{
    ("编码与换行（UTF-8 无 BOM、LF）", true, () => CheckEncoding(root, fix)),
    ("注释格式（summary 标签各占一行）", true, () => CheckCommentStyle(root, fix)),
    ("用词（设计文档编号、不用的说法）", true, () => CheckWording(root)),
    // 代码风格与分析器规则由编译检查（EnforceCodeStyleInBuild、警告即错误），这里只查编译不管的空白、文件末尾换行与 using 顺序
    ("格式（空白与 using 顺序）", true, () => RunWhenProjectsExist(root, "dotnet", "format Kmic.slnx --verify-no-changes --diagnostics IMPORTS")),
    ("编译（警告即错误）", true, () => RunWhenProjectsExist(root, "dotnet", "build Kmic.slnx --nologo")),
    ("测试", false, () => RunWhenProjectsExist(root, "dotnet", "test --solution Kmic.slnx --no-build")),
    ("依赖漏洞（含传递依赖）", false, () => WhenProjectsExist(root, () => CheckVulnerabilities(root))),
    // pnpm 版本与 web/package.json 保持一致
    // 接口类型比对依赖后端测试导出的 OpenAPI 文档，须在后端测试后执行
    ("前端依赖（pnpm install --frozen-lockfile）", true, () => RunWeb(root, "install --frozen-lockfile")),
    ("前端接口类型比对", false, () => RunWeb(root, "run generate:api -- --check")),
    ("前端类型检查", true, () => RunWeb(root, "run typecheck")),
    ("前端代码及依赖边界检查", true, () => RunWeb(root, "run lint")),
    ("前端格式（prettier --check）", true, () => RunWeb(root, "run format:check")),
    ("前端测试", true, () => RunWeb(root, "run test")),
    ("前端构建", false, () => RunWeb(root, "run build")),
    ("前端端到端（Playwright，假后端）", false, () => RunEndToEnd(root)),
    // 密钥扫描包含提交历史，CI 必须检出完整历史
    ("密钥扫描（Gitleaks）", false, () => Run(root, "docker", $"run --rm -v \"{root}:/repo\" ghcr.io/gitleaks/gitleaks:v8.30.1 git /repo --config /repo/.gitleaks.toml --redact --no-banner")),
    ("拼写（cspell）", false, () => Run(root, "npx", "--yes cspell@10.4.0 lint --no-progress --no-must-find-files --config cspell.json")),
};

var total = Stopwatch.StartNew();
foreach (var (name, _, run) in steps.Where(s => !fast || s.Fast))
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

Console.WriteLine(fast
    ? $"== 快速检查通过（共 {Seconds(total.Elapsed)} 秒；提交前仍要跑一次完整检查）"
    : $"== 全部通过（共 {Seconds(total.Elapsed)} 秒）");
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

// 浏览器装在用户目录，已装过时几乎不花时间；CI 的 Linux 执行机还要装系统依赖
static int RunEndToEnd(string root)
{
    var ci = string.Equals(Environment.GetEnvironmentVariable("CI"), "true", StringComparison.OrdinalIgnoreCase);
    var install = RunWeb(root, ci ? "exec playwright install --with-deps chromium" : "exec playwright install chromium");
    return install != 0 ? install : RunWeb(root, "run test:e2e");
}

static int Run(string root, string fileName, string arguments)
{
    // Windows 上的 npx 是批处理命令，需要通过 cmd.exe 执行
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

        // Windows 上的 NuGet 锁文件使用 CRLF，由 Git 入库时转为 LF
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

// 列出已跟踪文件和未被忽略的新文件，返回以 / 分隔的仓库相对路径
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

// 设计文档的节号、编号不写进代码：读代码的人不一定读过文档，文档改版后编号也会失效
static int CheckWording(string root)
{
    string[] extensions = [".cs", ".ts", ".vue"];
    var problems = 0;
    foreach (var file in ListFiles(root))
    {
        // 本脚本里的词表和正则就是规则本身，不检查
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
            if (Kmic.Scripts.Patterns.DesignReference().Match(lines[i]) is { Success: true } reference)
            {
                Console.WriteLine($"   {file}:{i + 1} 写了设计文档编号“{reference.Value}”，直接说明原因");
                problems++;
            }

            if (lines[i].Contains("【引用", StringComparison.Ordinal))
            {
                Console.WriteLine($"   {file}:{i + 1} 不再使用临时【引用】，直接说明原因");
                problems++;
            }

            if (Kmic.Scripts.Patterns.AvoidedWording().Match(lines[i]) is { Success: true } wording)
            {
                Console.WriteLine($"   {file}:{i + 1} “{wording.Value}”是设计文档里的说法或空话，换成直接的描述");
                problems++;
            }
        }
    }

    Console.WriteLine($"   问题 {problems} 处");
    return problems == 0 ? 0 : 1;
}

// 只检查整行的 XML 注释：summary 的开始、结束标签各占一行。句号不检查。修复时按原缩进拆成多行
static int CheckCommentStyle(string root, bool fix)
{
    var problems = 0;
    var fixedFiles = 0;
    foreach (var file in ListFiles(root).Where(f => f.EndsWith(".cs", StringComparison.OrdinalIgnoreCase)))
    {
        var path = Path.Combine(root, file);
        if (!File.Exists(path))
        {
            continue;
        }

        var lines = File.ReadAllLines(path);
        var output = new List<string>(lines.Length);
        var changed = false;
        for (var i = 0; i < lines.Length; i++)
        {
            if (!HasInlineSummaryTag(lines[i].Trim()))
            {
                output.Add(lines[i]);
                continue;
            }

            if (!fix)
            {
                Console.WriteLine($"   {file}:{i + 1} summary 的开始、结束标签要各占一行");
                problems++;
                continue;
            }

            output.AddRange(SplitSummaryTags(lines[i]));
            changed = true;
        }

        if (changed)
        {
            File.WriteAllText(path, string.Join('\n', output) + "\n", new UTF8Encoding(encoderShouldEmitUTF8Identifier: false));
            Console.WriteLine($"   已修复：{file}");
            fixedFiles++;
        }
    }

    Console.WriteLine(fix ? $"   已修复 {fixedFiles} 个文件" : $"   注释格式问题 {problems} 处");
    return problems == 0 ? 0 : 1;
}

static bool HasInlineSummaryTag(string line)
{
    if (!line.StartsWith("///", StringComparison.Ordinal))
    {
        return false;
    }

    var text = line[3..].Trim();
    return (text.StartsWith("<summary>", StringComparison.Ordinal) && text.Length > "<summary>".Length)
        || (text.EndsWith("</summary>", StringComparison.Ordinal) && text.Length > "</summary>".Length);
}

// "/// <summary>文字</summary>" 拆成三行；只有开始或结束标签和文字挤在一行的，拆成两行
static IEnumerable<string> SplitSummaryTags(string line)
{
    var indent = line[..line.IndexOf("///", StringComparison.Ordinal)];
    var text = line.Trim()[3..].Trim();
    var opens = text.StartsWith("<summary>", StringComparison.Ordinal);
    var closes = text.EndsWith("</summary>", StringComparison.Ordinal);
    var body = text[(opens ? "<summary>".Length : 0)..^(closes ? "</summary>".Length : 0)].Trim();
    if (opens)
    {
        yield return $"{indent}/// <summary>";
    }

    if (body.Length > 0)
    {
        yield return $"{indent}/// {body}";
    }

    if (closes)
    {
        yield return $"{indent}/// </summary>";
    }
}

namespace Kmic.Scripts
{
    internal static partial class Patterns
    {
        // 节号（第 5.3 节）、带文档号的决定（05-02 D5）、括号里的文档号（05-03 …）、原型编号、实施任务号
        [GeneratedRegex(@"第 ?\d+(\.\d+)* ?节|\b[FR]?\d{2}(-\d{2})? ?D\d+\b|（[FR]?\d{2}-\d{2}[ ）]|原型 ?#\d+|\bS\d-\d{2}\b|实施待办", RegexOptions.ExplicitCapture | RegexOptions.CultureInvariant, matchTimeoutMilliseconds: 1000)]
        public static partial Regex DesignReference();

        // 只收没有正当用法的说法：设计文档里的说法、自造词、空话。评审发现新的同类说法再加
        [GeneratedRegex("定稿|挂接|管道第|落地|收口|闭环|赋能|抓手|进行统一处理|高效管理|完整、灵活|完整性与可靠性", RegexOptions.CultureInvariant, matchTimeoutMilliseconds: 1000)]
        public static partial Regex AvoidedWording();
    }
}
