#:property RestorePackagesWithLockFile=false

// 本地与 CI 共用的质量检查：dotnet run scripts/check.cs
// 依次执行：编码与换行 → 注释格式 → 用词 → 格式 → 编译（零警告）→ 测试 → 依赖漏洞 → 前端（依赖、生成类型、类型检查、代码检查与边界样例、格式、测试、构建）→ 密钥扫描 → 拼写。任一步失败即停止，返回非零退出码
// 需要 Docker：数据库测试用测试容器，密钥扫描用 Gitleaks 的官方镜像
// CI 流水线只调用本脚本，本地通过即 CI 通过
// 加 -- --fix 时先去掉 BOM、把 CRLF 改为 LF 再检查。Windows 上的 dotnet sln、dotnet ef 等工具按 CRLF 写文件（迁移还带 BOM），用过之后运行一次。CI 不加这个参数
using System.Diagnostics;
using System.Text;
using System.Text.RegularExpressions;

var root = FindRepositoryRoot();
var fix = args.Contains("--fix", StringComparer.Ordinal);
var steps = new (string Name, Func<int> Run)[]
{
    ("编码与换行（UTF-8 无 BOM、LF）", () => CheckEncoding(root, fix)),
    ("注释格式（summary 三行、末尾不加句号）", () => CheckCommentStyle(root)),
    ("用词（设计文档编号、不用的说法）", () => CheckWording(root)),
    ("格式（dotnet format --verify-no-changes）", () => RunWhenProjectsExist(root, "dotnet", "format Kmic.slnx --verify-no-changes")),
    ("编译（警告即错误）", () => RunWhenProjectsExist(root, "dotnet", "build Kmic.slnx --nologo")),
    ("测试", () => RunWhenProjectsExist(root, "dotnet", "test --solution Kmic.slnx --no-build")),
    ("依赖漏洞（含传递依赖）", () => RunWhenProjectsExist(root, "dotnet", "list Kmic.slnx package --vulnerable --include-transitive")),
    // 前端用的 pnpm 版本与 web/package.json 的 packageManager 一致。生成类型的比对依赖后端测试导出的 OpenAPI 文档，所以排在后端测试之后
    ("前端依赖（pnpm install --frozen-lockfile）", () => RunWeb(root, "install --frozen-lockfile")),
    ("前端生成类型与 OpenAPI 文档一致", () => RunWeb(root, "run generate:api -- --check")),
    ("前端类型检查", () => RunWeb(root, "run typecheck")),
    ("前端代码检查与边界样例", () => RunWeb(root, "run lint")),
    ("前端格式（prettier --check）", () => RunWeb(root, "run format:check")),
    ("前端测试", () => RunWeb(root, "run test")),
    ("前端构建", () => RunWeb(root, "run build")),
    // 扫描全部提交历史，CI 检出时也要取完整历史。推送前就能发现，密钥不会离开本机
    ("密钥扫描（Gitleaks）", () => Run(root, "docker", $"run --rm -v \"{root}:/repo\" ghcr.io/gitleaks/gitleaks:v8.30.1 git /repo --config /repo/.gitleaks.toml --redact --no-banner")),
    ("拼写（cspell）", () => Run(root, "npx", "--yes cspell@10.4.0 lint --no-progress --no-must-find-files --config cspell.json")),
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

Console.WriteLine("== 全部通过");
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

// 空解决方案（阶段 0）没有项目时跳过 .NET 步骤
static int RunWhenProjectsExist(string root, string fileName, string arguments)
{
    var solution = File.ReadAllText(Path.Combine(root, "Kmic.slnx"));
    if (!solution.Contains("<Project ", StringComparison.Ordinal))
    {
        Console.WriteLine("   解决方案中还没有项目，跳过");
        return 0;
    }

    return Run(root, fileName, arguments);
}

// 没有 web 目录时跳过
static int RunWeb(string root, string pnpmArguments)
{
    if (!Directory.Exists(Path.Combine(root, "web")))
    {
        Console.WriteLine("   没有 web 目录，跳过");
        return 0;
    }

    return Run(root, "npx", $"--yes pnpm@11.28.9 --dir web {pnpmArguments}");
}

static int Run(string root, string fileName, string arguments)
{
    // Windows 上 npx 是批处理文件，要经 cmd 启动。不用外壳方式启动，输出才会留在当前控制台
    var startInfo = OperatingSystem.IsWindows() && string.Equals(fileName, "npx", StringComparison.Ordinal)
        ? new ProcessStartInfo("cmd.exe", $"/c npx {arguments}")
        : new ProcessStartInfo(fileName, arguments);
    startInfo.WorkingDirectory = root;
    startInfo.UseShellExecute = false;
    using var process = Process.Start(startInfo) ?? throw new InvalidOperationException($"无法启动 {fileName}。");
    process.WaitForExit();
    return process.ExitCode;
}

// 只检查 Git 跟踪与待提交的文本文件；二进制文件由 .gitattributes 声明
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

        // NuGet 在 Windows 上按 CRLF 生成锁定文件；入库时由 .gitattributes 统一为 LF
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
            Console.WriteLine($"   已规整：{file}");
            fixedFiles++;
            continue;
        }

        if (hasBom)
        {
            Console.WriteLine($"   带 BOM：{file}");
            problems++;
        }

        if (hasCrlf)
        {
            Console.WriteLine($"   CRLF 换行：{file}");
            problems++;
        }
    }

    Console.WriteLine(fix
        ? $"   检查 {files.Length} 个文件，规整 {fixedFiles} 个"
        : $"   检查 {files.Length} 个文件，问题 {problems} 处");
    return problems == 0 ? 0 : 1;
}

// Git 跟踪与待提交（未被忽略）的文件，路径相对仓库根目录、以 / 分隔
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

// 只检查整行注释：summary 的开始、结束标签各占一行，注释末尾不加句号（多行时看最后一行）
static int CheckCommentStyle(string root)
{
    var problems = 0;
    foreach (var file in ListFiles(root).Where(f => f.EndsWith(".cs", StringComparison.OrdinalIgnoreCase)))
    {
        var path = Path.Combine(root, file);
        if (!File.Exists(path))
        {
            continue;
        }

        var lines = File.ReadAllLines(path).Select(line => line.Trim()).ToArray();
        for (var i = 0; i < lines.Length; i++)
        {
            if (DescribeCommentProblem(lines, i) is { } problem)
            {
                Console.WriteLine($"   {file}:{i + 1} {problem}");
                problems++;
            }
        }
    }

    Console.WriteLine($"   注释格式问题 {problems} 处");
    return problems == 0 ? 0 : 1;
}

static string? DescribeCommentProblem(string[] lines, int index)
{
    var line = lines[index];
    if (line.StartsWith("///", StringComparison.Ordinal))
    {
        var text = line[3..].Trim();
        var opensWithText = text.StartsWith("<summary>", StringComparison.Ordinal) && text.Length > "<summary>".Length;
        var closesAfterText = text.EndsWith("</summary>", StringComparison.Ordinal) && text.Length > "</summary>".Length;
        if (opensWithText || closesAfterText)
        {
            return "summary 的开始、结束标签要各占一行";
        }

        // summary 正文的最后一行：下一行是结束标签
        var isLastSummaryLine = index + 1 < lines.Length
            && string.Equals(lines[index + 1], "/// </summary>", StringComparison.Ordinal)
            && !text.StartsWith('<');
        return isLastSummaryLine && text.EndsWith('。') ? "注释末尾不加句号" : null;
    }

    if (line.StartsWith("//", StringComparison.Ordinal))
    {
        var isLastCommentLine = index + 1 >= lines.Length || !lines[index + 1].StartsWith("//", StringComparison.Ordinal);
        return isLastCommentLine && line.EndsWith('。') ? "注释末尾不加句号" : null;
    }

    return null;
}

namespace Kmic.Scripts
{
    internal static partial class Patterns
    {
        // 节号（第 5.3 节）、带文档号的决定（05-02 D5）、括号里的文档号（05-03 …）、原型编号、实施任务号
        [GeneratedRegex(@"第 ?\d+(\.\d+)* ?节|\b[FR]?\d{2}(-\d{2})? ?D\d+\b|（[FR]?\d{2}-\d{2}[ ）]|原型 ?#\d+|\bS\d-\d{2}\b|实施待办", RegexOptions.ExplicitCapture | RegexOptions.CultureInvariant, matchTimeoutMilliseconds: 1000)]
        public static partial Regex DesignReference();

        // 只收没有正当用法的说法：设计文档里的说法、自造词、空话。评审发现新的同类说法再加
        [GeneratedRegex("定稿|挂接|租约|管道第|落地|收口|闭环|赋能|抓手|进行统一处理|高效管理|完整、灵活|完整性与可靠性", RegexOptions.CultureInvariant, matchTimeoutMilliseconds: 1000)]
        public static partial Regex AvoidedWording();
    }
}
