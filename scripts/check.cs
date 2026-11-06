#:property RestorePackagesWithLockFile=false

// 本地与 CI 共用的质量检查：dotnet run scripts/check.cs
// 依次执行：编码与换行 → 注释格式 → 临时设计引用的格式 → 格式 → 编译（零警告）→ 测试 → 依赖漏洞 → 密钥扫描 → 拼写；任一步失败即停止并返回非零退出码
// 需要 Docker：数据库测试用测试容器，密钥扫描用 Gitleaks 的官方镜像
// CI 流水线只调用本脚本，本地通过即 CI 通过
// 加 -- --fix 时先去掉 BOM、把 CRLF 改为 LF 再检查：Windows 上的 dotnet sln、dotnet ef 等工具按 CRLF 写文件（迁移还带 BOM），用过之后运行一次；CI 不加
using System.Diagnostics;
using System.Text;

var root = FindRepositoryRoot();
var fix = args.Contains("--fix", StringComparer.Ordinal);
var steps = new (string Name, Func<int> Run)[]
{
    ("编码与换行（UTF-8 无 BOM、LF，06 第 3.2 节）", () => CheckEncoding(root, fix)),
    ("注释格式（summary 三行、末尾不加句号）", () => CheckCommentStyle(root)),
    ("临时设计引用的格式（06 第 10.3 节）", () => CheckDesignCitations(root)),
    ("格式（dotnet format --verify-no-changes）", () => RunWhenProjectsExist(root, "dotnet", "format Kmic.slnx --verify-no-changes")),
    ("编译（警告即错误）", () => RunWhenProjectsExist(root, "dotnet", "build Kmic.slnx --nologo")),
    ("测试", () => RunWhenProjectsExist(root, "dotnet", "test --solution Kmic.slnx --no-build")),
    ("依赖漏洞（含传递依赖）", () => RunWhenProjectsExist(root, "dotnet", "list Kmic.slnx package --vulnerable --include-transitive")),
    // 扫描全部提交历史（CI 检出时要取完整历史）；在推送之前发现，密钥就不会离开本机
    ("密钥扫描（Gitleaks，ADR-0032）", () => Run(root, "docker", $"run --rm -v \"{root}:/repo\" ghcr.io/gitleaks/gitleaks:v8.30.1 git /repo --config /repo/.gitleaks.toml --redact --no-banner")),
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

    return directory?.FullName ?? throw new InvalidOperationException("Kmic.slnx not found; run from inside the repository.");
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

static int Run(string root, string fileName, string arguments)
{
    // Windows 上 npx 是批处理文件，需经 cmd 启动；不用外壳方式启动，以便输出留在当前控制台
    var startInfo = OperatingSystem.IsWindows() && string.Equals(fileName, "npx", StringComparison.Ordinal)
        ? new ProcessStartInfo("cmd.exe", $"/c npx {arguments}")
        : new ProcessStartInfo(fileName, arguments);
    startInfo.WorkingDirectory = root;
    startInfo.UseShellExecute = false;
    using var process = Process.Start(startInfo) ?? throw new InvalidOperationException($"Cannot start {fileName}.");
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
    using var git = Process.Start(startInfo) ?? throw new InvalidOperationException("Cannot start git.");
    var files = git.StandardOutput.ReadToEnd().Split('\0', StringSplitOptions.RemoveEmptyEntries);
    git.WaitForExit();
    return files;
}

// 临时设计引用只能写在 // 注释行末，前面必须有原因；阶段 2 验收删除全部引用后，改为不允许出现
static int CheckDesignCitations(string root)
{
    string[] extensions = [".cs", ".ts", ".vue"];
    var citations = 0;
    var problems = 0;
    foreach (var file in ListFiles(root))
    {
        // 本脚本中的标记是检查规则本身
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
            var marker = lines[i].IndexOf("【引用", StringComparison.Ordinal);
            if (marker < 0)
            {
                continue;
            }

            citations++;
            if (DescribeCitationProblem(lines[i], marker) is { } problem)
            {
                Console.WriteLine($"   {file}:{i + 1} {problem}");
                problems++;
            }
        }
    }

    Console.WriteLine($"   临时引用 {citations} 处，格式问题 {problems} 处");
    return problems == 0 ? 0 : 1;
}

static string? DescribeCitationProblem(string line, int marker)
{
    // 从标记往前找最近的 //；“///” 中最近的一个 // 前面还有一个 /
    var comment = line.LastIndexOf("//", marker, StringComparison.Ordinal);
    if (comment < 0)
    {
        return "不在 // 注释中";
    }

    if (comment > 0 && line[comment - 1] == '/')
    {
        return "不能写在 /// XML 注释中";
    }

    if (line[(comment + 2)..marker].Trim().Length == 0)
    {
        return "引用前面没有写原因";
    }

    const string Prefix = "【引用：";
    var citation = line[marker..].TrimEnd();
    var closing = citation.IndexOf('】', StringComparison.Ordinal);
    if (!citation.StartsWith(Prefix, StringComparison.Ordinal) || closing != citation.Length - 1 || closing == Prefix.Length)
    {
        return "格式应为行末的【引用：文档与节号】";
    }

    return null;
}

// 只检查整行注释：summary 的开始、结束标签各占一行；注释（多行时看最后一行）末尾不加句号
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
