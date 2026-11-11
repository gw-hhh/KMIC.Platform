using Kmic.BuildingBlocks.Application.Messaging;
using Kmic.BuildingBlocks.Domain.Results;

namespace Kmic.BuildingBlocks.Application.Tests.Fixtures;

// 正式代码的命令处理器不能注入 ISender，这里故意违反，验证运行时的拦截
public sealed class SendInsideCommandHandler(ISender sender) : ICommandHandler<SendInsideCommand>
{
    public async ValueTask<Result> Handle(SendInsideCommand command, CancellationToken cancellationToken)
    {
        if (command.SendCommand)
        {
            return await sender.SendAsync(new ChangePasswordCommand(), cancellationToken);
        }

        var queried = await sender.SendAsync(new GetThingQuery(), cancellationToken);
        return queried.IsSuccess ? Result.Success() : queried.Error;
    }
}
