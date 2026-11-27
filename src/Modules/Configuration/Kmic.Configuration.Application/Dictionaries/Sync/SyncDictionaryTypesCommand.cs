using Kmic.BuildingBlocks.Application.Messaging;
using Kmic.BuildingBlocks.Application.UseCases;

namespace Kmic.Configuration.Application.Dictionaries.Sync;

/// <summary>将代码声明的字典类型同步至数据库</summary>
[SystemOnly]
[NotAudited("目录同步，结果记入启动日志")]
public sealed record SyncDictionaryTypesCommand : ICommand<DictionarySyncResult>;
