var builder = DistributedApplication.CreateBuilder(args);

// 版本与生产环境的 PostgreSQL 小版本一致；数据放在命名卷里，重启后还在
var postgres = builder.AddPostgres("postgres")
    .WithContainerName("kmic-test-postgres-aspire")
    .WithImageTag("18.6")
    .WithDataVolume("kmic-test-postgres-aspire-data")
    .WithInitFiles("init");
var database = postgres.AddDatabase("kmic");

// 先迁移，成功后再启动应用（ADR-0022 的部署顺序）。开发环境用容器的管理账号；运行账号的权限收紧由集成测试覆盖
var migrator = builder.AddProject<Projects.Kmic_MigrationRunner>("migrator")
    .WithEnvironment("Kmic__Migration__ConnectionString", database.Resource.ConnectionStringExpression)
    .WaitFor(database);

// 当前开发编排仅配置 Worker 角色；接口联调还需同时启用 Api 角色
builder.AddProject<Projects.Kmic_Api>("api")
    .WithEnvironment("Kmic__Roles", "Worker")
    .WithEnvironment("Kmic__Database__ConnectionString", database.Resource.ConnectionStringExpression)
    .WaitForCompletion(migrator);

await builder.Build().RunAsync();
