using Kmic.BuildingBlocks.Application.Messaging;
using Kmic.BuildingBlocks.Application.UseCases;

namespace Kmic.Operations.Application.Messages.Rebuilds;

/// <summary>
/// 确认收尾：源端找不到的行太多而暂停的一轮，运维核实之后确认它们确实没有了
/// </summary>
/// <remarks>
/// 确认之后另排一个作业只做收尾：把这些行置为终态，完成这一轮。不确定就别确认，核实之前可以放弃
/// </remarks>
/// <param name="Subscription">订阅名</param>
[RequirePermission(OperationsPermissions.MessagesManage)]
[Audited]
[Idempotent]
public sealed record ConfirmRebuildCommand(string Subscription) : ICommand;
