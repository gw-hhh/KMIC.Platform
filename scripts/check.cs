#:property RestorePackagesWithLockFile=false

// 本地和 CI 共用的质量检查：dotnet run scripts/check.cs
// 按下方顺序执行，任一步失败即停止，并返回该步骤的退出码。
// 数据库测试和密钥扫描需要 Docker。
// 本地可加 -- --fix 移除 BOM，并将 CRLF 换行改为 LF；CI 只检查，不修复。
using System.Diagnostics;
using System.Text;
using System.Text.RegularExpressions;

var root = FindRepositoryRoot();
var fix = args.Contains("--fix", StringComparer.Ordinal);
var steps = new (string Name, Func<int> Run)[]
{
    ("编码与换行（UTF-8 无 BOM、LF）", () => CheckEncoding(root, fix)),
    ("临时引用标记", () => CheckTemporaryReferences(root)),
    ("格式（dotnet format --verify-no-changes）", () => RunWhenProjectsExist(root, "dotnet", "format Kmic.slnx --verify-no-changes")),
    ("编译（警告即错误）", () => RunWhenProjectsExist(root, "dotnet", "build Kmic.slnx --nologo")),
    ("测试", () => RunWhenProjectsExist(root, "dotnet", "test --solution Kmic.slnx --no-build")),
    ("依赖漏洞（含传递依赖）", () => RunWhenProjectsExist(root, "dotnet", "list Kmic.slnx package --vulnerable --include-transitive")),
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

static int RunWhenProjectsExist(string root, string fileName, string arguments)
{
    var solution = File.ReadAllText(Path.Combine(root, "Kmic.slnx"));
    if (!solution.Contains("<Project ", StringComparison.Ordinal))
    {
        Console.WriteLine("   解决方案中没有项目，跳过 .NET 检查");
        return 0;
    }

    return Run(root, fileName, arguments);
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
