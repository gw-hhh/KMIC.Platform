#:property RestorePackagesWithLockFile=false
#:property PublishAot=false

// 本机运行：dotnet run scripts/dev.cs -- up
// 起数据库容器 → 建库与运行账号 → 迁移 → 首次建管理员 → 启动 Api（含后台处理）。再次运行会复用已有的库和账号。
// 需要 Docker 和 .NET 10 SDK。生成的密码、签名密钥、上传的文件都放在仓库根目录的 .local 文件夹里，不进仓库。
// 命令：up（默认，在前台运行 Api，Ctrl+C 停止）｜ status（看现在是什么状态）｜ reset（删掉本机的库，下次 up 重新建）
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

var root = FindRepositoryRoot();
var local = Path.Combine(root, ".local");
Directory.CreateDirectory(local);
var statePath = Path.Combine(local, "dev.json");
var state = LoadState(statePath);

var command = args.FirstOrDefault(a => !a.StartsWith('-')) ?? "up";
return command switch
{
    "up" => await UpAsync(),
    "status" => Status(),
    "reset" => Reset(),
    _ => Fail($"不认识的命令 {command}。可用：up、status、reset"),
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
        return Fail("迁移失败，上面有原因。");
    }

    EnsureAdministrator(superConnection);
    EnsureSigningKey();

    Console.WriteLine("== 启动 Api");
    Console.WriteLine($"   接口文档：http://localhost:{ApiPort}/scalar");
    Console.WriteLine($"   管理员账号：{AdminAccount}（密码见 {Path.Combine(".local", "dev.json")}，首次登录要先改密码）");
    Console.WriteLine("   Ctrl+C 停止；数据库容器会继续运行");
    var runtime = RuntimeConnection(Get("AppPassword"));
    return RunDotnet("src/Hosts/Kmic.Api", [], Env(
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
        ("Files__Storage__LocalRoot", Path.Combine(local, "files"))));
}

int Status()
{
    var exists = Capture("docker", ["ps", "-a", "--filter", $"name=^{Container}$", "--format", "{{.Status}}"]).Output.Trim();
    Console.WriteLine($"数据库容器 {Container}：{(exists.Length == 0 ? "还没有" : exists)}");
    if (exists.StartsWith("Up", StringComparison.Ordinal))
    {
        var built = string.Equals(Psql("postgres", $"SELECT 1 FROM pg_database WHERE datname = '{Database}'").Output.Trim(), "1", StringComparison.Ordinal);
        Console.WriteLine($"数据库 {Database}：{(built ? "已建" : "还没有")}");
        if (built)
        {
            var users = Psql(Database, "SELECT count(*) FROM \"Identity\".\"User\" WHERE \"Kind\" = 'Normal'").Output.Trim();
            Console.WriteLine($"用户数：{(users.Length == 0 ? "还没迁移" : users)}");
        }
    }

    Console.WriteLine($"Api 地址：http://localhost:{ApiPort}（运行时才有）");
    return 0;
}

int Reset()
{
    EnsureDocker();
    var status = Capture("docker", ["ps", "--filter", $"name=^{Container}$", "--format", "{{.Names}}"]).Output.Trim();
    if (status.Length == 0)
    {
        return Fail("数据库容器没有在运行，没有可删的库。");
    }

    var dropped = Psql("postgres", $"DROP DATABASE IF EXISTS {Database} WITH (FORCE)");
    if (dropped.ExitCode != 0)
    {
        return Fail(dropped.Error);
    }

    Set("AdminPassword", null);
    Console.WriteLine($"已删除数据库 {Database}。再运行 up 会重新迁移并建管理员。");
    return 0;
}

void EnsureDocker()
{
    var info = Capture("docker", ["info", "--format", "{{.ServerVersion}}"]);
    if (info.ExitCode != 0)
    {
        Console.WriteLine("Docker 没有运行：先打开 Docker Desktop，等它就绪再试。");
        Environment.Exit(1);
    }
}

// 数据库容器：已经有就启动，没有就新建。返回超级用户密码
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
        // 手工建的容器，密码在它的环境变量里
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

    Fail("数据库 60 秒内没有就绪。");
    return password;
}

string ReadContainerPassword()
{
    var env = Capture("docker", ["inspect", Container, "--format", "{{range .Config.Env}}{{println .}}{{end}}"]).Output;
    var line = env.Split('\n').FirstOrDefault(l => l.StartsWith("POSTGRES_PASSWORD=", StringComparison.Ordinal));
    if (line is null)
    {
        Fail($"读不到容器 {Container} 的数据库密码。");
        return string.Empty;
    }

    return line["POSTGRES_PASSWORD=".Length..].Trim();
}

void EnsureDatabaseAndRoles()
{
    if (!string.Equals(Psql("postgres", $"SELECT 1 FROM pg_database WHERE datname = '{Database}'").Output.Trim(), "1", StringComparison.Ordinal))
    {
        Console.WriteLine($"== 建数据库 {Database}");
        Check(Psql("postgres", $"CREATE DATABASE {Database}"));
    }

    // 运行账号：迁移里的授权要求 kmic_runtime 组角色先存在，应用用它的成员 kmic_app 登录
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
    // 系统里自带一个内置的 system 用户，只数真人用户
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
        Fail("创建管理员失败，上面有原因。");
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
    Console.WriteLine("== 已生成本机用的令牌签名密钥");
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
    // 不用 Visual Studio 生成的启动配置，端口和环境变量以这里给的为准
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

// 字符表去掉了容易看混的 I、l、O、0、1
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

    throw new InvalidOperationException("找不到仓库根目录（Kmic.slnx）。请在仓库里运行。");
}

// 本机状态（生成的密码）存在 .local/dev.json，一个名字对一个值
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
