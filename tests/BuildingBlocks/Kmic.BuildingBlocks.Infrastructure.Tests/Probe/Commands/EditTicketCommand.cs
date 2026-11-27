using Kmic.BuildingBlocks.Application.Messaging;
using Kmic.BuildingBlocks.Application.UseCases;

namespace Kmic.BuildingBlocks.Infrastructure.Tests.Probe.Commands;

/// <summary>
/// 按测试传入的委托修改单据，用于验证审计记录生成。
/// </summary>
/// <param name="Id">单据</param>
/// <param name="Edit">修改单据的方法</param>
/// <param name="Detail">补充到审计记录里的说明</param>
/// <param name="Remove">软删除单据</param>
/// <param name="Restore">恢复被软删除的单据</param>
[AllowAuthenticated]
[Audited(Action = "BuildingBlocks.EditTicket")]
internal sealed record EditTicketCommand(TicketId Id, Action<Ticket>? Edit = null, string? Detail = null, bool Remove = false, bool Restore = false) : ICommand;
