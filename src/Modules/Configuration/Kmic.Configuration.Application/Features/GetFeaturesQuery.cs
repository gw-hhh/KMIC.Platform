using Kmic.BuildingBlocks.Application.Messaging;
using Kmic.BuildingBlocks.Application.UseCases;

namespace Kmic.Configuration.Application.Features;

/// <summary>查询已开启的功能开关</summary>
[AllowAuthenticated]
public sealed record GetFeaturesQuery : IQuery<FeaturesResponse>;
