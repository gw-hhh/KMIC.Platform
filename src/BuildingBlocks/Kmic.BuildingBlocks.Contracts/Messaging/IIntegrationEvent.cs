namespace Kmic.BuildingBlocks.Contracts.Messaging;

/// <summary>集成事件；须携带订阅方所需的全部数据，订阅方不得回查发布方。</summary>
public interface IIntegrationEvent : IIntegrationMessage;
