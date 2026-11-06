namespace Spike01.BuildingBlocks;

// 每个 DI 作用域一个；跨模块调用期间由 ContractQuerySender 通过 AsyncLocal 临时覆盖来源（05-07 第 4.2 节）
internal sealed class ExecutionContext : IExecutionContext
{
    private static readonly AsyncLocal<CallOrigin?> _override = new();

    public static CallOrigin? Override
    {
        get => _override.Value;
        set => _override.Value = value;
    }

    public CallOrigin BaseOrigin { get; set; } = CallOrigin.Http;

    public CallOrigin Origin => _override.Value ?? BaseOrigin;
}
