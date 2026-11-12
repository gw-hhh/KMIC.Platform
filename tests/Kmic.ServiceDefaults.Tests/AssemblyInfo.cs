// AddSerilog 会把新建的日志器写进全局的 Log.Logger，请求日志中间件也从那里取；
// 并行的测试会互相覆盖，所以这个项目的全部测试放进同一个集合，逐个运行
[assembly: CollectionBehavior(CollectionBehavior.CollectionPerAssembly)]
