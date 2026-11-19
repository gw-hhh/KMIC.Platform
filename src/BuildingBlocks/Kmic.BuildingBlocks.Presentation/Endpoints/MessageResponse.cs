using Kmic.BuildingBlocks.Application.Jobs;
using Kmic.BuildingBlocks.Application.Messaging;

namespace Kmic.BuildingBlocks.Presentation.Endpoints;

/// <summary>
/// 消息成功时的 HTTP 响应：状态码和响应体类型
/// </summary>
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
