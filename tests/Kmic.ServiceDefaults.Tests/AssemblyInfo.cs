// AddSerilog 会将新建日志器写入全局 Log.Logger，请求日志中间件亦从该全局实例读取；
// 为避免并行测试相互覆盖全局状态，本测试项目中的所有测试均放入同一集合顺序执行
[assembly: CollectionBehavior(CollectionBehavior.CollectionPerAssembly)]
