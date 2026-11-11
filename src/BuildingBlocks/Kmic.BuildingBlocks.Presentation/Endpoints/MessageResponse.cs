using Kmic.BuildingBlocks.Application.Jobs;
using Kmic.BuildingBlocks.Application.Messaging;

namespace Kmic.BuildingBlocks.Presentation.Endpoints;

/// <summary>
/// 消息成功时的 HTTP 响应：状态码和响应体类型
/// </summary>
/// <remarks>
/// 与 ResultHttpExtensions 的写法一一对应：没有返回值的命令 204，返回创建结果的 201，返回作业受理的 202，
/// 其余 200 加返回值。端点返回 IResult，框架生成不出响应类型，所以由 Sends 按这个规则声明
/// </remarks>
internal static class MessageResponse
{
    public static (int Status, Type? Body) Of(Type message)
    {
        var body = message.GetInterfaces()
            .Where(i => i.IsGenericType && (i.GetGenericTypeDefinition() == typeof(ICommand<>) || i.GetGenericTypeDefinition() == typeof(IQuery<>)))
            .Select(i => i.GetGenericArguments()[0])
            .FirstOrDefault();

        return body switch
        {
            null => (StatusCodes.Status204NoContent, null),
            { IsGenericType: true } when body.GetGenericTypeDefinition() == typeof(CreatedResponse<>) => (StatusCodes.Status201Created, body),
            _ when body == typeof(JobAccepted) => (StatusCodes.Status202Accepted, body),
            _ => (StatusCodes.Status200OK, body),
        };
    }
}
