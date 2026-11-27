#:property RestorePackagesWithLockFile=false
#:property PublishAot=false

// 本机运行：dotnet run scripts/dev.cs -- up
// 启动数据库容器 → 初始化库与运行账号 → 执行迁移 → 首次创建管理员 → 启动 Api（含后台任务）。重复执行会复用已有库与账号。
// 依赖 Docker 与 .NET 10 SDK。生成的密码、签名密钥及本地上传文件存放于仓库根目录 .local 下（不纳入版本控制）。
// 命令：up（默认，前台运行 Api，Ctrl+C 停止）｜ status（查看当前环境状态）｜ reset（删除本地数据库，下次 up 时重建）
// up --dashboard：额外启动可观测面板（Aspire Dashboard 独立镜像）并接收 Api 遥测数据，访问 http://localhost:18888
using System.Diagnostics;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

const string Container = "kmic-test-postgres";
const string DataVolume = "kmic-test-postgres-data";
const string PostgresImage = "postgres:18.6";
const string Database = "kmic_local";
const int DatabasePort = 5433;
const int ApiPort = 5080;
const string AdminAccount = "admin";
const string DashboardContainer = "kmic-test-dashboard";
const string DashboardImage = "mcr.microsoft.com/dotnet/aspire-dashboard:13.6.5";
const int DashboardPort = 18888;
const int OtlpPort = 4317;

var root = FindRepositoryRoot();
var local = Path.Combine(root, ".local");
Directory.CreateDirectory(local);
var statePath = Path.Combine(local, "dev.json");
var state = LoadState(statePath);

var command = args.FirstOrDefault(a => !a.StartsWith('-')) ?? "up";
var dashboard = args.Contains("--dashboard", StringComparer.Ordinal);
return command switch
{
    "up" => await UpAsync(),
    "status" => Status(),
    "reset" => Reset(),
    _ => Fail($"未知命令 {command}。可用：up、status、reset"),
};

async Task<int> UpAsync()
{
    EnsureDocker();
    var superPassword = EnsureDatabaseContainer();
    EnsureDatabaseAndRoles();
    var superConnection = SuperConnection(superPassword);

    Console.WriteLine("== 迁移");
    var migrate = RunDotnet("src/Hosts/Kmic.MigrationRunner", ["migrate"], Env(("Kmic__Migration__ConnectionString", superConnection), ("DOTNET_ENVIRONMENT", "Development")));
    if (migrate != 0)
    {
        return Fail("迁移失败，详见上方输出。");
    }

    EnsureAdministrator(superConnection);
    EnsureSigningKey();
    if (dashboard && !EnsureDashboard())
    {
        return Fail("可观测面板启动失败，详见上方输出。");
    }

    Console.WriteLine("== 启动 Api");
    Console.WriteLine($"   接口文档：http://localhost:{ApiPort}/scalar");
    Console.WriteLine($"   管理员账号：{AdminAccount}（密码见 {Path.Combine(".local", "dev.json")}，首次登录后须修改密码）");
    Console.WriteLine("   Ctrl+C 停止；数据库容器会继续运行");
    var runtime = RuntimeConnection(Get("AppPassword"));
    // 仅在启用面板时导出遥测；Kmic.ServiceDefaults 检测到此环境变量后开启 OTLP 导出
    (string, string)[] telemetry = dashboard
        ? [("OTEL_EXPORTER_OTLP_ENDPOINT", $"http://localhost:{OtlpPort}"), ("OTEL_SERVICE_NAME", "kmic-api")]
        : [];
    return RunDotnet("src/Hosts/Kmic.Api", [], Env([.. telemetry,
        ("ASPNETCORE_ENVIRONMENT", "Development"),
        ("ASPNETCORE_URLS", $"http://localhost:{ApiPort}"),
        ("Kmic__Database__ConnectionString", runtime),
        ("Kmic__Database__MigrationConnectionString", superConnection),
        ("Kmic__Roles", "Api,Worker"),
        ("Kmic__Auth__Issuer", "kmic-local"),
        ("Kmic__Auth__Audience", "kmic-local"),
        ("Kmic__Auth__SigningKeys__0__Kid", "local-1"),
        ("Kmic__Auth__SigningKeys__0__Status", "Active"),
        ("Kmic__Auth__SigningKeys__0__PrivateKeyPath", Path.Combine(local, "keys", "private.pem")),
        // 前端开发服务器地址，用于刷新令牌时的来源校验
        ("Identity__Cookie__TrustedOrigins__0", "http://localhost:5173"),
        ("Identity__Cookie__TrustedOrigins__1", "http://127.0.0.1:5173"),
        ("Files__Storage__LocalRoot", Path.Combine(local, "files"))]));
}

// 面板仅绑定本机回环地址且免登录，限本地开发使用；数据存储在容器内存中，重启后清空
bool EnsureDashboard()
{
    Console.WriteLine("== 可观测面板");
    var existing = Capture("docker", ["ps", "-a", "--filter", $"name=^{DashboardContainer}$", "--format", "{{.Status}}"]).Output.Trim();
    var result = existing.Length == 0
        ? Capture("docker", ["run", "-d", "--name", DashboardContainer,
            "-p", $"127.0.0.1:{DashboardPort}:18888", "-p", $"127.0.0.1:{OtlpPort}:18889",
            "-e", "Dashboard__Frontend__AuthMode=Unsecured", "-e", "Dashboard__Otlp__AuthMode=Unsecured",
            DashboardImage])
        : existing.StartsWith("Up", StringComparison.Ordinal) ? (ExitCode: 0, Output: "", Error: "") : Capture("docker", ["start", DashboardContainer]);
    if (result.ExitCode != 0)
    {
        Console.WriteLine(result.Error);
        return false;
    }

    Console.WriteLine($"   http://localhost:{DashboardPort}");
    return true;
}

int Status()
{
    var exists = Capture("docker", ["ps", "-a", "--filter", $"name=^{Container}$", "--format", "{{.Status}}"]).Output.Trim();
    Console.WriteLine($"数据库容器 {Container}：{(exists.Length == 0 ? "未创建" : exists)}");
    if (exists.StartsWith("Up", StringComparison.Ordinal))
    {
        var built = string.Equals(Psql("postgres", $"SELECT 1 FROM pg_database WHERE datname = '{Database}'").Output.Trim(), "1", StringComparison.Ordinal);
        Console.WriteLine($"数据库 {Database}：{(built ? "已创建" : "未创建")}");
        if (built)
        {
            var users = Psql(Database, "SELECT count(*) FROM \"Identity\".\"User\" WHERE \"Kind\" = 'Normal'").Output.Trim();
            Console.WriteLine($"用户数：{(users.Length == 0 ? "未取得用户数，请检查迁移状态和数据库连接" : users)}");
        }
    }

    Console.WriteLine($"Api 地址：http://localhost:{ApiPort}（仅在运行时可用）");
    var panel = Capture("docker", ["ps", "-a", "--filter", $"name=^{DashboardContainer}$", "--format", "{{.Status}}"]).Output.Trim();
    Console.WriteLine($"可观测面板 {DashboardContainer}：{(panel.Length == 0 ? "未启动（需通过 up --dashboard 启动）" : $"{panel}，http://localhost:{DashboardPort}")}");
    return 0;
}

int Reset()
{
    EnsureDocker();
    var status = Capture("docker", ["ps", "--filter", $"name=^{Container}$", "--format", "{{.Names}}"]).Output.Trim();
    if (status.Length == 0)
    {
        return Fail("数据库容器未运行，无法删除数据库。请先启动容器。");
    }

    var dropped = Psql("postgres", $"DROP DATABASE IF EXISTS {Database} WITH (FORCE)");
    if (dropped.ExitCode != 0)
    {
        return Fail(dropped.Error);
    }

    Set("AdminPassword", null);
    Console.WriteLine($"已删除数据库 {Database}，下次执行 up 时将重新迁移并创建管理员。");
    return 0;
}

void EnsureDocker()
{
    var info = Capture("docker", ["info", "--format", "{{.ServerVersion}}"]);
    if (info.ExitCode != 0)
    {
        Console.WriteLine("Docker 未运行，请先启动 Docker Desktop 并等待就绪后重试。");
        Environment.Exit(1);
    }
}

// 确保数据库容器存在并处于运行状态（不存在时新建），返回超级用户密码
string EnsureDatabaseContainer()
{
    var existing = Capture("docker", ["ps", "-a", "--filter", $"name=^{Container}$", "--format", "{{.Status}}"]).Output.Trim();
    string password;
    if (existing.Length == 0)
    {
        password = Get("PostgresPassword") ?? RandomPassword();
        Console.WriteLine($"== 新建数据库容器 {Container}（端口 {DatabasePort}）");
        var created = Capture("docker",
        [
            "run", "-d", "--name", Container,
            "-p", $"127.0.0.1:{DatabasePort}:5432",
            "-v", $"{DataVolume}:/var/lib/postgresql",
            "-e", $"POSTGRES_PASSWORD={password}",
            PostgresImage,
            "-c", "max_connections=200",
        ]);
        if (created.ExitCode != 0)
        {
            Fail(created.Error);
        }
    }
    else
    {
        // 本机未保存密码时，从容器环境变量读取
        password = Get("PostgresPassword") ?? ReadContainerPassword();
        if (!existing.StartsWith("Up", StringComparison.Ordinal))
        {
            Console.WriteLine($"== 启动数据库容器 {Container}");
            var started = Capture("docker", ["start", Container]);
            if (started.ExitCode != 0)
            {
                Fail(started.Error);
            }
        }
    }

    if (!string.Equals(Get("PostgresPassword"), password, StringComparison.Ordinal))
    {
        Set("PostgresPassword", password);
    }

    for (var attempt = 0; attempt < 60; attempt++)
    {
        if (Capture("docker", ["exec", Container, "pg_isready", "-U", "postgres"]).ExitCode == 0)
        {
            return password;
        }

        Thread.Sleep(1000);
    }

    Fail("数据库在 60 秒内未就绪。");
    return password;
}

string ReadContainerPassword()
{
    var env = Capture("docker", ["inspect", Container, "--format", "{{range .Config.Env}}{{println .}}{{end}}"]).Output;
    var line = env.Split('\n').FirstOrDefault(l => l.StartsWith("POSTGRES_PASSWORD=", StringComparison.Ordinal));
    if (line is null)
    {
        Fail($"无法读取容器 {Container} 的数据库密码。");
        return string.Empty;
    }

    return line["POSTGRES_PASSWORD=".Length..].Trim();
}

void EnsureDatabaseAndRoles()
{
    if (!string.Equals(Psql("postgres", $"SELECT 1 FROM pg_database WHERE datname = '{Database}'").Output.Trim(), "1", StringComparison.Ordinal))
    {
        Console.WriteLine($"== 创建数据库 {Database}");
        Check(Psql("postgres", $"CREATE DATABASE {Database}"));
    }

    // 运行账号：迁移脚本中的授权依赖 kmic_runtime 组角色，应用运行时通过其成员 kmic_app 登录
    var appPassword = Get("AppPassword") ?? RandomPassword();
    Check(Psql("postgres", "DO $$ BEGIN IF NOT EXISTS (SELECT 1 FROM pg_roles WHERE rolname = 'kmic_runtime') THEN CREATE ROLE kmic_runtime NOLOGIN; END IF; END $$"));
    Check(Psql("postgres", "DO $$ BEGIN IF NOT EXISTS (SELECT 1 FROM pg_roles WHERE rolname = 'kmic_app') THEN CREATE ROLE kmic_app LOGIN IN ROLE kmic_runtime; END IF; END $$"));
    Check(Psql("postgres", $"ALTER ROLE kmic_app PASSWORD '{appPassword}'"));
    if (!string.Equals(Get("AppPassword"), appPassword, StringComparison.Ordinal))
    {
        Set("AppPassword", appPassword);
    }
}

void EnsureAdministrator(string superConnection)
{
    // 忽略内置系统账号，仅统计普通用户
    var users = Psql(Database, "SELECT count(*) FROM \"Identity\".\"User\" WHERE \"Kind\" = 'Normal'").Output.Trim();
    if (!string.Equals(users, "0", StringComparison.Ordinal))
    {
        return;
    }

    Console.WriteLine("== 创建首个管理员");
    var password = RandomPassword();
    var code = RunDotnet(
        "src/Hosts/Kmic.Operator",
        ["identity", "bootstrap", "--account", AdminAccount, "--name", "系统管理员"],
        Env(("Kmic__Operator__ConnectionString", superConnection), ("DOTNET_ENVIRONMENT", "Development")),
        password);
    if (code != 0)
    {
        Fail("创建管理员失败，详见上方输出。");
        return;
    }

    Set("AdminPassword", password);
    Console.WriteLine($"   账号 {AdminAccount}，密码已写入 {Path.Combine(".local", "dev.json")}");
}

void EnsureSigningKey()
{
    var keys = Path.Combine(local, "keys");
    var privatePath = Path.Combine(keys, "private.pem");
    if (File.Exists(privatePath))
    {
        return;
    }

    Directory.CreateDirectory(keys);
    using var rsa = RSA.Create(2048);
    File.WriteAllText(privatePath, rsa.ExportPkcs8PrivateKeyPem());
    File.WriteAllText(Path.Combine(keys, "public.pem"), rsa.ExportSubjectPublicKeyInfoPem());
    Console.WriteLine("== 已生成本机令牌签名密钥");
}

string SuperConnection(string password) => $"Host=127.0.0.1;Port={DatabasePort};Database={Database};Username=postgres;Password={password}";

string RuntimeConnection(string? password) => $"Host=127.0.0.1;Port={DatabasePort};Database={Database};Username=kmic_app;Password={password}";

(int ExitCode, string Output, string Error) Psql(string database, string sql) =>
    Capture("docker", ["exec", Container, "psql", "-U", "postgres", "-d", database, "-v", "ON_ERROR_STOP=1", "-tAc", sql]);

void Check((int ExitCode, string Output, string Error) result)
{
    if (result.ExitCode != 0)
    {
        Fail(result.Error);
    }
}

int Fail(string message)
{
    Console.Error.WriteLine(message);
    Environment.Exit(1);
    return 1;
}

static Dictionary<string, string> Env(params (string Name, string Value)[] pairs) => pairs.ToDictionary(p => p.Name, p => p.Value, StringComparer.Ordinal);

int RunDotnet(string project, string[] programArgs, Dictionary<string, string> environment, string? stdin = null)
{
    var startInfo = new ProcessStartInfo("dotnet") { WorkingDirectory = root, UseShellExecute = false, RedirectStandardInput = stdin is not null };
    // 忽略 launchSettings.json，端口与环境变量以脚本传入为准
    foreach (var part in new[] { "run", "--no-launch-profile", "--project", project, "--" }.Concat(programArgs))
    {
        startInfo.ArgumentList.Add(part);
    }

    foreach (var (name, value) in environment)
    {
        startInfo.Environment[name] = value;
    }

    using var process = Process.Start(startInfo) ?? throw new InvalidOperationException("无法启动 dotnet。");
    if (stdin is not null)
    {
        process.StandardInput.WriteLine(stdin);
        process.StandardInput.Close();
    }

    process.WaitForExit();
    return process.ExitCode;
}

static (int ExitCode, string Output, string Error) Capture(string fileName, string[] arguments)
{
    var startInfo = new ProcessStartInfo(fileName) { UseShellExecute = false, RedirectStandardOutput = true, RedirectStandardError = true, StandardOutputEncoding = Encoding.UTF8, StandardErrorEncoding = Encoding.UTF8 };
    foreach (var argument in arguments)
    {
        startInfo.ArgumentList.Add(argument);
    }

    using var process = Process.Start(startInfo) ?? throw new InvalidOperationException($"无法启动 {fileName}。");
    var output = new StringBuilder();
    var error = new StringBuilder();
    process.OutputDataReceived += (_, e) => output.AppendLine(e.Data);
    process.ErrorDataReceived += (_, e) => error.AppendLine(e.Data);
    process.BeginOutputReadLine();
    process.BeginErrorReadLine();
    process.WaitForExit();
    return (process.ExitCode, output.ToString(), error.ToString());
}

// 字符表剔除了易混淆字符 I、l、O、0、1
// cspell:disable-next-line
static string RandomPassword() => RandomNumberGenerator.GetString("ABCDEFGHJKLMNPQRSTUVWXYZabcdefghijkmnopqrstuvwxyz23456789", 20) + "#7";

static string FindRepositoryRoot()
{
    for (var candidate = new DirectoryInfo(Directory.GetCurrentDirectory()); candidate is not null; candidate = candidate.Parent)
    {
        if (File.Exists(Path.Combine(candidate.FullName, "Kmic.slnx")))
        {
            return candidate.FullName;
        }
    }

    throw new InvalidOperationException("找不到仓库根目录（Kmic.slnx）。请在仓库目录下运行。");
}

string? Get(string key) => state.GetValueOrDefault(key);

void Set(string key, string? value)
{
    if (value is null)
    {
        state.Remove(key);
    }
    else
    {
        state[key] = value;
    }

    File.WriteAllText(statePath, JsonSerializer.Serialize(state, new JsonSerializerOptions { WriteIndented = true }));
}

static Dictionary<string, string> LoadState(string path) =>
    File.Exists(path)
        ? JsonSerializer.Deserialize<Dictionary<string, string>>(File.ReadAllText(path)) ?? new Dictionary<string, string>(StringComparer.Ordinal)
        : new Dictionary<string, string>(StringComparer.Ordinal);
